using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public abstract class BaseGraphDbClient : IGraphDbClient
{
    private bool _disposed;

    protected readonly ILogger Logger;

    public bool IsDisposed => _disposed;

    protected BaseGraphDbClient(ILogger logger)
    {
        Logger = logger;
    }

    public abstract IGraphUnitOfWorkFactory UnitOfWorkFactory { get; }

    public virtual async ValueTask<IGraphUnitOfWork> BeginUnitOfWorkAsync()
    {
        return await UnitOfWorkFactory.CreateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        await InternalDisposeAsync();
        
        GC.SuppressFinalize(this);
    }

    public abstract ValueTask<T> ExecuteReadAsync<T>(Func<IQueryRunner, ValueTask<T>> operation);
    public abstract ValueTask<T> ExecuteWriteAsync<T>(Func<IQueryRunner, ValueTask<T>> operation);

    protected virtual ValueTask InternalDisposeAsync() => ValueTask.CompletedTask;
}
