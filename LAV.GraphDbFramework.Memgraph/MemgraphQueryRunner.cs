using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Mapping;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Memgraph;

internal sealed class MemgraphQueryRunner : IQueryRunner
{
    private readonly IAsyncTransaction? _transaction;
    private readonly ILogger? _logger;
    private static readonly ConcurrentDictionary<Type, Func<Core.IRecord, object>> MapperCache = [];

    public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters = null)
    {
        return await RunAsync(query, parameters, record =>
        {
            // Используем кэшированный маппер
            if (MapperCache.TryGetValue(typeof(T), out var mapper))
                return (T)mapper(record);

            // Ищем сгенерированный маппер
            var mapperType = typeof(T).Assembly.GetType($"{typeof(T).Namespace}.{typeof(T).Name}Mapper");
            if (mapperType?.GetMethod("MapFromRecord", BindingFlags.Public | BindingFlags.Static) is MethodInfo method)
            {
                var mapperFunc = (Func<Core.IRecord, T>)Delegate.CreateDelegate(typeof(Func<Core.IRecord, T>), method);
                MapperCache[typeof(T)] = record => mapperFunc(record)!;
                return mapperFunc(record);
            }

            // Используем общий кэш мапперов
            return MapperCache<T>.MapFromRecord(record);
        });
    }

    public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<Core.IRecord, T> mapper)
    {
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("MemgraphQuery");
        activity?.SetTag("db.query", query);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _transaction!.RunAsync(query, parameters);
            var records = await result.ToListAsync();

            activity?.SetTag("db.duration", stopwatch.Elapsed);

            _logger?.Debug("Memgraph query executed in {ElapsedMs}ms: {Query}", stopwatch.Elapsed, query);

            var results = new List<T>(records.Count);
            foreach (var record in records)
            {
                results.Add(mapper(new MemgraphRecord(record)));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error executing Memgraph query: {Query}", query);
            throw;
        }
    }
}
