using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Memgraph;

public abstract class BaseQueryRunner : IQueryRunner
{
    private bool _disposed;
    private IDisposable? _loggerScope = null;

    protected readonly ILogger Logger;

    public bool IsDisposed => _disposed;

    protected BaseQueryRunner(ILogger logger)
    {
        Logger = logger;
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

    public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);

    public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<IRecord, T>? mapper);
}