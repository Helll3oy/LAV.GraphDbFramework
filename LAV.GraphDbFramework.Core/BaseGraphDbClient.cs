using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public abstract class BaseGraphDbClient<TGraphDbOptions, TGraphDbQueryRunner, TGraphDbUnitOfWork, TGraphDbUnitOfWorkFactory>
    : IGraphDbClient
    where TGraphDbOptions : IGraphDbOptions
    where TGraphDbQueryRunner : IGraphDbQueryRunner
    where TGraphDbUnitOfWork : IGraphDbUnitOfWork
    where TGraphDbUnitOfWorkFactory : IGraphDbUnitOfWorkFactory
{
    private bool _disposed;
    private readonly IOptionsMonitor<TGraphDbOptions> _monitor;
    //private readonly TGraphDbOptions _options;
    private IDisposable? _subscription = null;

    protected readonly ILogger Logger;
    protected TGraphDbOptions Options;

    public bool IsDisposed => _disposed;

    protected BaseGraphDbClient(IOptionsMonitor<TGraphDbOptions> monitor, ILogger logger)
    {
        Logger = logger;

        _monitor = monitor;
        Options = monitor.CurrentValue;

        _subscription = monitor.OnChange(newOptions =>
        {
            lock (Options)
            {
                if (Options.ConnectionTimeout != newOptions.ConnectionTimeout)
                {
                    Options.ConnectionTimeout = newOptions.ConnectionTimeout;
                }
            }
        });
    }

    public abstract TGraphDbUnitOfWorkFactory UnitOfWorkFactory { get; }
    IGraphDbUnitOfWorkFactory IGraphDbClient.UnitOfWorkFactory => UnitOfWorkFactory;

    public abstract ValueTask<TGraphDbUnitOfWork> BeginUnitOfWorkAsync();
    async ValueTask<IGraphDbUnitOfWork> IGraphDbClient.BeginUnitOfWorkAsync()
    {
        return await BeginUnitOfWorkAsync().ConfigureAwait(false);
    }

    public abstract ValueTask<T> ExecuteReadAsync<T>(Func<TGraphDbQueryRunner, ValueTask<T>> operation);
    async ValueTask<T> IGraphDbClient.ExecuteReadAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
    {
        return await ExecuteReadAsync<T>(queryRunner => operation(queryRunner));
    }

    public abstract ValueTask<T> ExecuteWriteAsync<T>(Func<TGraphDbQueryRunner, ValueTask<T>> operation);
    async ValueTask<T> IGraphDbClient.ExecuteWriteAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
    {
        return await ExecuteWriteAsync<T>(queryRunner => operation(queryRunner));
    }

    protected virtual ValueTask InternalDisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _subscription?.Dispose();
        _subscription = null;

        _disposed = true;

        await InternalDisposeAsync();

        GC.SuppressFinalize(this);
    }
}
