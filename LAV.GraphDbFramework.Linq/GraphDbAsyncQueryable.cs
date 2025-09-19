using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Linq;

public class GraphDbAsyncQueryable<T> : IGraphDbAsyncQueryable<T>, IAsyncEnumerable<T>
{
	private readonly IGraphDbQueryProvider _provider;
	private readonly Expression _expression;

	public GraphDbAsyncQueryable(IGraphDbQueryProvider provider, Expression expression)
	{
		_provider = provider;
		_expression = expression;
	}

	public Type ElementType => typeof(T);
	public Expression Expression => _expression;
	public IGraphDbQueryProvider Provider => _provider;

	IQueryProvider IQueryable.Provider => Provider;

	public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return new AsyncEnumerator<T>(this, cancellationToken);
	}

	//private List<T> _cachedItems;
	//private readonly object _cacheLock = new object();

	public IEnumerator<T> GetEnumerator()
	{
		// Для синхронного доступа выполняем запрос синхронно
		var result = _provider.Execute<List<T>>(_expression);
		return result?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();

		//throw new NotSupportedException("Synchronous enumeration is not supported. Use GetAsyncEnumerator instead.");

		//return new SynchronousEnumeratorAdapter<T>(this);
		// или
		//lock (_cacheLock)
		//{
		//	if (_cachedItems == null)
		//	{
		//		_cachedItems = Task.Run(async () =>
		//		{
		//			var list = new List<T>();
		//			await foreach (var item in this.WithCancellation(CancellationToken.None).ConfigureAwait(false))
		//			{
		//				list.Add(item);
		//			}
		//			return list;
		//		}).GetAwaiter().GetResult();
		//	}

		//	return _cachedItems.GetEnumerator();
		//}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	//// Вспомогательный класс для адаптации асинхронного перечисления к синхронному
	//private class SynchronousEnumeratorAdapter<T> : IEnumerator<T>
	//{
	//	private readonly IAsyncEnumerable<T> _asyncEnumerable;
	//	private IAsyncEnumerator<T> _asyncEnumerator;
	//	private T _current;

	//	public SynchronousEnumeratorAdapter(IAsyncEnumerable<T> asyncEnumerable)
	//	{
	//		_asyncEnumerable = asyncEnumerable;
	//	}

	//	public T Current => _current;

	//	object IEnumerator.Current => Current;

	//	public bool MoveNext()
	//	{
	//		if (_asyncEnumerator == null)
	//		{
	//			_asyncEnumerator = _asyncEnumerable.GetAsyncEnumerator();
	//		}

	//		// Синхронное ожидание асинхронной операции
	//		var moveNextTask = Task.Run(async () => await _asyncEnumerator.MoveNextAsync());
	//		var hasNext = moveNextTask.GetAwaiter().GetResult();

	//		if (hasNext)
	//		{
	//			_current = _asyncEnumerator.Current;
	//		}

	//		return hasNext;
	//	}

	//	public void Reset()
	//	{
	//		// Сброс не поддерживается для асинхронных перечислений
	//		throw new NotSupportedException("Reset is not supported for asynchronous enumerators");
	//	}

	//	public void Dispose()
	//	{
	//		if (_asyncEnumerator != null)
	//		{
	//			// Синхронное ожидание асинхронной операции dispose
	//			Task.Run(async () => await _asyncEnumerator.DisposeAsync()).GetAwaiter().GetResult();
	//		}
	//	}
	//}

	// Методы для построения запросов
	
	public IGraphDbAsyncQueryable<T> Where(Expression<Func<T, bool>> predicate)
	{
		if (predicate == null)
			throw new ArgumentNullException(nameof(predicate));

		var whereExpression = Expression.Call(
			null,
			GetMethodInfo(nameof(Queryable.Where), [typeof(T)], this, predicate),
			Expression.Quote(predicate)
		);

		return new GraphDbAsyncQueryable<T>(_provider, whereExpression);
	}

	public IGraphDbAsyncQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
	{
		if (keySelector == null)
			throw new ArgumentNullException(nameof(keySelector));

		var orderByExpression = Expression.Call(
			null,
			GetMethodInfo(nameof(Queryable.OrderBy), [typeof(T), typeof(TKey)], this, keySelector),
			Expression.Quote(keySelector)
		);

		return new GraphDbAsyncQueryable<T>(_provider, orderByExpression);
	}

	public IGraphDbAsyncQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
	{
		if (keySelector == null)
			throw new ArgumentNullException(nameof(keySelector));

		var orderByDescExpression = Expression.Call(
			null,
			GetMethodInfo(nameof(Queryable.OrderByDescending), [typeof(T), typeof(TKey)], this, keySelector),
			Expression.Quote(keySelector)
		);

		return new GraphDbAsyncQueryable<T>(_provider, orderByDescExpression);
	}

	public IGraphDbAsyncQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
	{
		if (selector == null)
			throw new ArgumentNullException(nameof(selector));

		var selectExpression = Expression.Call(
			null,
			GetMethodInfo(nameof(Queryable.Select), [typeof(T), typeof(TResult)], this, selector),
			new Expression[] { Expression, Expression.Quote(selector) }
		);

		return new GraphDbAsyncQueryable<TResult>(_provider, selectExpression);
	}

	private static MethodInfo GetMethodInfo(string methodName, Type[] genericTypes, params object[] parameters)
	{
		return typeof(Queryable).GetMethods()
			.First(m => m.Name == methodName && m.GetParameters().Length == parameters.Length)
			.MakeGenericMethod(genericTypes);
	}

	// Внутренний класс для асинхронного перечисления
	private class AsyncEnumerator<TResult> : IAsyncEnumerator<TResult>
	{
		private readonly GraphDbAsyncQueryable<TResult> _queryable;
		private readonly CancellationToken _cancellationToken;
		private IEnumerator<TResult> _enumerator;

		public AsyncEnumerator(GraphDbAsyncQueryable<TResult> queryable, CancellationToken cancellationToken)
		{
			_queryable = queryable;
			_cancellationToken = cancellationToken;
		}

		public TResult Current => _enumerator.Current;

		public async ValueTask<bool> MoveNextAsync()
		{
			if (_enumerator == null)
			{
				// Выполняем запрос асинхронно
				var results = await _queryable._provider.ExecuteAsync<List<TResult>>(
					_queryable.Expression, _cancellationToken);
				_enumerator = results?.GetEnumerator() ?? Enumerable.Empty<TResult>().GetEnumerator();
			}

			return _enumerator.MoveNext();
		}

		public ValueTask DisposeAsync()
		{
			_enumerator?.Dispose();
			return ValueTask.CompletedTask;
		}
	}
}