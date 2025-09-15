using LAV.GraphDbFramework.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public interface IGraphClient : IAsyncDisposable
{
    ValueTask<T> ExecuteReadAsync<T>(Func<IQueryRunner, ValueTask<T>> operation);
    ValueTask<T> ExecuteWriteAsync<T>(Func<IQueryRunner, ValueTask<T>> operation);
    ValueTask<IGraphUnitOfWork> BeginUnitOfWorkAsync();
    IGraphUnitOfWorkFactory UnitOfWorkFactory { get; }
}