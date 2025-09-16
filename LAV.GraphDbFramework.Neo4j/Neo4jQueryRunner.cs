using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Extensions;
using LAV.GraphDbFramework.Core.Mapping;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Neo4j;

internal sealed class Neo4jQueryRunner : IQueryRunner
{
    private readonly IAsyncQueryRunner _runner;
    private readonly ILogger<Neo4jQueryRunner> _logger;
	private readonly ILoggerFactory _loggerFactory;

	private static readonly ConcurrentDictionary<Type, Func<Core.IRecord, object>> MapperCache = [];
	public Neo4jQueryRunner(IAsyncQueryRunner runner, ILoggerFactory loggerFactory)
	{
		_loggerFactory = loggerFactory;
		_runner = runner;
		_logger = loggerFactory.CreateLogger<Neo4jQueryRunner>(); 
	}

	public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters = null)
        where T : class, new()
    {
		return await RunAsync(query, parameters, record =>
		{
			// Используем кэшированный маппер, если доступен
			if (MapperCache.TryGetValue(typeof(T), out var mapper))
				return (T)mapper(record);

			// Пытаемся найти сгенерированный маппер
			var generatedMapperType = typeof(T).Assembly.GetType($"{typeof(T).Namespace}.{typeof(T).Name}Mapper");
			if (generatedMapperType?.GetMethod("MapFromRecord", BindingFlags.Public | BindingFlags.Static) is MethodInfo method)
			{
				var mapperFunc = (Func<Core.IRecord, T>)Delegate.CreateDelegate(typeof(Func<Core.IRecord, T>), method);
				MapperCache[typeof(T)] = record => mapperFunc(record);
				return mapperFunc(record);
			}

			// Используем общий кэш мапперов
			return MapperCache<T>.MapFromRecord(record);
		});
	}

    public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<Core.IRecord, T> mapper)
		where T : class, new()
	{
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Neo4jQuery");
        activity?.SetTag("db.query", query);

        var stopwatch = Stopwatch.StartNew();

        try
        {
			IDictionary<string, object>? queryParams = null;

			if (parameters is IDictionary<string, object> dictParams)
			{
				queryParams = dictParams;
			}
			else if (parameters != null)
			{
				// Конвертируем анонимный объект в словарь с использованием пула
				using var pooledDict = new PooledDictionary();
				var properties = pooledDict.Dictionary;

				foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					var value = prop.GetValue(parameters);
					if(value != null) properties[prop.Name] = value;
				}

				queryParams = properties;
			}

			var result = await _runner!.RunAsync(query, queryParams);
            var records = await result.ToListAsync();

            activity?.SetTag("db.duration", stopwatch.Elapsed);

            _logger.LogDebug("Neo4j query executed in {ElapsedMs}ms: {Query}", stopwatch.Elapsed, query);

            var results = new List<T>(records.Count);
            foreach (var record in records)
            {
                results.Add(mapper(new Neo4jRecord(record)));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Neo4j query: {Query}", query);
            throw;
        }
    }
}
