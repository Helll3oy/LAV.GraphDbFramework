using LAV.GraphDbFramework.Core.Mapping;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Memgraph.UnitOfWork;

public sealed class MemgraphUnitOfWork : IGraphUnitOfWork
{
    private readonly IAsyncSession _session;
    private readonly IAsyncTransaction _transaction;

	private readonly ILoggerFactory _loggerFactory;
	private readonly ILogger<MemgraphUnitOfWork> _logger;
    private bool _disposed;
    private bool _committed;

    public bool IsDisposed => _disposed;
    public bool IsCommitted => _committed;

    public MemgraphUnitOfWork(IDriver driver, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<MemgraphUnitOfWork>();
        _session = driver.AsyncSession();
        _transaction = _session.BeginTransactionAsync().GetAwaiter().GetResult();
    }

    public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters = null)
        where T : class, new()
    {
        ThrowIfDisposed();

        var result = await _transaction.RunAsync(query, parameters);
        var records = await result.ToListAsync();

        var results = new List<T>(records.Count);
        foreach (var record in records)
        {
            results.Add(MapperCache<T>.MapFromRecord(new MemgraphRecord(record)));
        }

		return results;
    }

    public async ValueTask CommitAsync()
    {
        ThrowIfDisposed();

        await _transaction.CommitAsync();
        _committed = true;
        _logger?.LogDebug("Neo4j UnitOfWork committed");
    }

    public async ValueTask RollbackAsync()
    {
        ThrowIfDisposed();

        await _transaction.RollbackAsync();
        _logger?.LogDebug("Neo4j UnitOfWork rolled back");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (!_committed)
        {
            try
            {
                await RollbackAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during rollback on dispose");
            }
        }

        await _transaction.DisposeAsync();
        await _session.DisposeAsync();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed)
            return;

        throw new ObjectDisposedException(nameof(MemgraphUnitOfWork));
    }

    public ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<Core.IRecord, T> mapper)
		where T : class, new()
	{
        throw new NotImplementedException();
    }
}