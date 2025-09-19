
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Linq;

public interface IGraphDbQueryProvider : IQueryProvider
{
	ValueTask<T> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken = default);
	IGraphDbAsyncQueryable<T> CreateAsyncQuery<T>();
	IGraphDbAsyncQueryable<T> CreateAsyncQuery<T>(Expression expression);
}
