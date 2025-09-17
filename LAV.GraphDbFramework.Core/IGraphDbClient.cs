using LAV.GraphDbFramework.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public interface IGraphDbClient : IAsyncDisposable
{
    ValueTask<T> ExecuteReadAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation);
    ValueTask<T> ExecuteWriteAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation);
    ValueTask<IGraphDbUnitOfWork> BeginUnitOfWorkAsync();
    IGraphDbUnitOfWorkFactory UnitOfWorkFactory { get; }
}