using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.RetryPolicies;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core;

public abstract class BaseGraphDbQueryRunner<TGraphDbRecord> : IGraphDbQueryRunner
	where TGraphDbRecord : IGraphDbRecord
{
    private bool _disposed;
    private IDisposable? _loggerScope = null;

	protected readonly IGraphDbRetryPolicy RetryPolicy;
	protected readonly ILogger Logger;

    public bool IsDisposed => _disposed;

    protected BaseGraphDbQueryRunner(ILogger logger, IGraphDbRetryPolicy? retryPolicy = null)
    {
        Logger = logger;
		RetryPolicy = retryPolicy ?? new NoRetryPolicy();
	}

    protected void BeginLoggerScope<T>(T state) where T : notnull
    {
        _loggerScope = Logger.BeginScope(state);
    }

    private void EndLoggerScope()
    {
        if (_loggerScope == null) return;

        _loggerScope?.Dispose();
        _loggerScope = null;
    }

    protected virtual ValueTask InternalDisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        await InternalDisposeAsync();

        EndLoggerScope();

        GC.SuppressFinalize(this);
    }

    public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<TGraphDbRecord, T> mapper);
	public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);

    async ValueTask<IReadOnlyList<T>> IGraphDbQueryRunner.RunAsync<T>(string query, object? parameters, Func<IGraphDbRecord, T> mapper)
    {
        return await RunAsync<T>(query, parameters, record => mapper(record)).ConfigureAwait(false);
    }
}