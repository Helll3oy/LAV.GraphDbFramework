using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Linq;

public static class GraphDbAsyncQueryableExtensions
{
	public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IGraphDbAsyncQueryable<T> source,
		CancellationToken cancellationToken = default)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		// Выполняем запрос и берем первый элемент
		var results = await source.ToListAsync(cancellationToken);
		return results.FirstOrDefault();

		//// Создаем выражение для FirstOrDefault
		//var methodInfo = typeof(Queryable).GetMethods()
		//	.First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1)
		//	.MakeGenericMethod(typeof(T));

		//var expression = Expression.Call(null, methodInfo, source.Expression);

		//// Выполняем запрос через провайдер
		//var provider = source.Provider as IGraphDbQueryProvider;
		//if (provider == null)
		//	throw new InvalidOperationException("The query provider must implement IGraphQueryProvider");

		//return await provider.ExecuteAsync<T>(expression, cancellationToken);
	}

	public static async Task<List<T>> ToListAsync<T>(this IGraphDbAsyncQueryable<T> source,
		CancellationToken cancellationToken = default)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		//// Создаем выражение для ToList
		//var methodInfo = typeof(Enumerable).GetMethods()
		//	.First(m => m.Name == "ToList" && m.GetParameters().Length == 1)
		//	.MakeGenericMethod(typeof(T));

		//var expression = Expression.Call(null, methodInfo, source.Expression);

		// Выполняем запрос через провайдер
		var provider = source.Provider as IGraphDbQueryProvider;
		if (provider == null)
			throw new InvalidOperationException("The query provider must implement IGraphQueryProvider");

		var result = await provider.ExecuteAsync<List<T>>(source.Expression, cancellationToken);
		return result ?? [];
	}

	public static async Task<int> CountAsync<T>(this IGraphDbAsyncQueryable<T> source,
		CancellationToken cancellationToken = default)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		// Создаем выражение для Count
		var methodInfo = typeof(Queryable).GetMethods()
			.First(m => m.Name == "Count" && m.GetParameters().Length == 1)
			.MakeGenericMethod(typeof(T));

		var expression = Expression.Call(null, methodInfo, source.Expression);

		// Выполняем запрос через провайдер
		var provider = source.Provider as IGraphDbQueryProvider;
		if (provider == null)
			throw new InvalidOperationException("The query provider must implement IGraphQueryProvider");

		return await provider.ExecuteAsync<int>(expression, cancellationToken);
	}

	public static async Task<bool> AnyAsync<T>(this IGraphDbAsyncQueryable<T> source,
		CancellationToken cancellationToken = default)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		// Создаем выражение для Any
		var methodInfo = typeof(Queryable).GetMethods()
			.First(m => m.Name == "Any" && m.GetParameters().Length == 1)
			.MakeGenericMethod(typeof(T));

		var expression = Expression.Call(null, methodInfo, source.Expression);

		// Выполняем запрос через провайдер
		var provider = source.Provider as IGraphDbQueryProvider;
		if (provider == null)
			throw new InvalidOperationException("The query provider must implement IGraphQueryProvider");

		return await provider.ExecuteAsync<bool>(expression, cancellationToken);
	}
}