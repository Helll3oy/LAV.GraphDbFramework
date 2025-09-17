using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public abstract class BaseGraphDbClient<TGraphDbOptions, TGraphDbQueryRunner, TGraphDbRecord, TGraphDbUnitOfWork, TGraphDbUnitOfWorkFactory> 
    : IGraphDbClient
    where TGraphDbOptions : IGraphDbOptions
	where TGraphDbRecord : IGraphDbRecord
	where TGraphDbQueryRunner : IGraphDbQueryRunner<TGraphDbRecord>
	where TGraphDbUnitOfWork : IGraphDbUnitOfWork<TGraphDbRecord>
	where TGraphDbUnitOfWorkFactory : IGraphDbUnitOfWorkFactory<TGraphDbUnitOfWork, TGraphDbRecord>
{
    private bool _disposed;
	private readonly IOptionsMonitor<TGraphDbOptions> _monitor;
	private readonly TGraphDbOptions _options;
	private IDisposable? _subscription = null;
	private readonly TGraphDbUnitOfWorkFactory _unitOfWorkFactory;

	protected readonly ILogger Logger;
	protected TGraphDbOptions Options => _options;

	public bool IsDisposed => _disposed;

    protected BaseGraphDbClient(IOptionsMonitor<TGraphDbOptions> monitor, ILogger logger)
    {
		Logger = logger;

		_monitor = monitor;
        _options = monitor.CurrentValue;

        _subscription = monitor.OnChange(newOptions =>
        {
            lock (_options)
            {
                if (_options.ConnectionTimeout != newOptions.ConnectionTimeout)
                {
					_options.ConnectionTimeout = newOptions.ConnectionTimeout;
                }
            }
        });
    }

    public TGraphDbUnitOfWorkFactory UnitOfWorkFactory => _unitOfWorkFactory;

	IGraphDbUnitOfWorkFactory IGraphDbClient.UnitOfWorkFactory => throw new NotImplementedException();

	public virtual async ValueTask<TGraphDbUnitOfWork> BeginUnitOfWorkAsync()
    {
        return await UnitOfWorkFactory.CreateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _subscription?.Dispose();
        _subscription = null;

		_disposed = true;

        await InternalDisposeAsync();
        
        GC.SuppressFinalize(this);
    }

    public abstract ValueTask<T> ExecuteReadAsync<T>(Func<TGraphDbQueryRunner, ValueTask<T>> operation);
    public abstract ValueTask<T> ExecuteWriteAsync<T>(Func<TGraphDbQueryRunner, ValueTask<T>> operation);

    protected virtual ValueTask InternalDisposeAsync() => ValueTask.CompletedTask;

	public ValueTask<T> ExecuteReadAsync<T>(Func<Core.IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		throw new NotImplementedException();
	}

	public ValueTask<T> ExecuteWriteAsync<T>(Func<Core.IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		throw new NotImplementedException();
	}

	ValueTask<UnitOfWork.IGraphDbUnitOfWork> IGraphDbClient.BeginUnitOfWorkAsync()
	{
		throw new NotImplementedException();
	}
}
