using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Linq;

public class CypherQueryProvider : IGraphDbQueryProvider
{
	private readonly IGraphDbClient _graphClient;
	private readonly ILogger<CypherQueryProvider> _logger;
	private readonly CypherExpressionTranslator _translator;

	public CypherQueryProvider(IGraphDbClient graphClient, CypherExpressionTranslator? translator, ILogger<CypherQueryProvider>? logger = null)
	{
		_graphClient = graphClient;
		_logger = logger!;
		_translator = translator ?? new CypherExpressionTranslator();
	}

	public IQueryable<T> CreateQuery<T>(Expression expression)
	{
		return new GraphDbAsyncQueryable<T>(this, expression);
	}
	
	public IQueryable CreateQuery(Expression expression)
	{
		var elementType = GetElementType(expression.Type);
		try
		{
			return (IQueryable)Activator.CreateInstance(
				typeof(GraphDbAsyncQueryable<>).MakeGenericType(elementType),
				this, expression);
		}
		catch (Exception ex)
		{
			throw new GraphDbQueryException("Failed to create query", new{ }, ex);
		}
	}

	private static Type GetElementType(Type type)
	{
		// Проверяем, является ли тип массивом
		if (type.IsArray)
			return type.GetElementType();

		// Проверяем, реализует ли тип IEnumerable<T>
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			return type.GetGenericArguments()[0];

		// Пытаемся найти интерфейс IEnumerable<T> в наследовании
		var ienum = type.GetInterfaces()
			.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			.Select(t => t.GetGenericArguments()[0])
			.FirstOrDefault();

		if (ienum != null)
			return ienum;

		// Если это интерфейс IQueryable (без generic параметра)
		if (type == typeof(IQueryable))
			return typeof(object);

		// Если это IQueryable<T>
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
			return type.GetGenericArguments()[0];

		// Если ничего не подходит, возвращаем сам тип
		return type;
	}

	public T Execute<T>(Expression expression)
	{
		return ExecuteAsync<T>(expression).GetAwaiter().GetResult();
	}

	public object Execute(Expression expression)
	{
		return ExecuteAsync<object>(expression).GetAwaiter().GetResult();
	}


	public async ValueTask<T> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken = default)
	{
		(string Query, IReadOnlyDictionary<string, object> Parameters) cypherQuery = default;
		try
		{
			// Преобразуем Expression в Cypher-запрос
			var translator = new CypherExpressionTranslator();
			cypherQuery = translator.Translate(expression);

			_logger?.LogDebug("Translated LINQ to Cypher: {CypherQuery}", cypherQuery.Query);

			// Определяем тип результата
			if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
			{
				// Для списков выполняем запрос и возвращаем все результаты
				var results = await _graphClient.ExecuteReadAsync(async runner =>
				{
					return await runner.RunAsync<T>(cypherQuery.Query, cypherQuery.Parameters);
				});

				// Создаем экземпляр списка через рефлексию
				var listType = typeof(List<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
				var list = Activator.CreateInstance(listType) as IList;

				if (results != null)
				{
					foreach (var item in results)
					{
						list.Add(item);
					}
				}

				return (T)list;
			}
			else
			{
				// Для одиночных результатов возвращаем первый элемент
				var results = await _graphClient.ExecuteReadAsync(async runner =>
				{
					return await runner.RunAsync<T>(cypherQuery.Query, cypherQuery.Parameters);
				});

				return results.FirstOrDefault();
			}
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Failed to execute LINQ query");
			throw new GraphDbLinqException("LINQ query with execution failed", 
				new { cypherQuery.Query, cypherQuery.Parameters }, ex);
		}
	}
	[Obsolete]
	public async ValueTask<T> ExecuteAsync_old<T>(Expression expression, CancellationToken cancellationToken = default)
	{
		(string Query, IReadOnlyDictionary<string, object> Parameters) cypherQuery = default;
		try
		{
			// Преобразуем Expression в Cypher-запрос
			//var translator = new CypherExpressionTranslator();
			cypherQuery = _translator.Translate(expression);

			_logger?.LogDebug("Translated LINQ to Cypher: {CypherQuery}", cypherQuery.Query);

			// Выполняем запрос
			return await _graphClient.ExecuteReadAsync(async runner =>
			{
				var results = await runner.RunAsync<T>(cypherQuery.Query, cypherQuery.Parameters);
				return results.FirstOrDefault();
			});
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Failed to execute LINQ query");
			throw new GraphDbLinqException("LINQ query with execution failed", new { cypherQuery.Query, cypherQuery.Parameters  }, ex);
		}
	}

	public IGraphDbAsyncQueryable<T> CreateAsyncQuery<T>()
	{
		return CreateAsyncQuery<T>(Expression.Constant(this));
	}

	public IGraphDbAsyncQueryable<T> CreateAsyncQuery<T>(Expression expression)
	{
		return new GraphDbAsyncQueryable<T>(this, expression);
	}
}