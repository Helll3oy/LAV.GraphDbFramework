using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Linq;

public interface IGraphDbAsyncQueryable<T> : IQueryable<T>
{
	IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
	IGraphDbAsyncQueryable<T> Where(Expression<Func<T, bool>> predicate);
	IGraphDbAsyncQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
	IGraphDbAsyncQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
	IGraphDbAsyncQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
}

