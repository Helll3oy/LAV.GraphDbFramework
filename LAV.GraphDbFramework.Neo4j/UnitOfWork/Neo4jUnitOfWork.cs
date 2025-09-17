using LAV.GraphDbFramework.Core.Mapping;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Neo4j.UnitOfWork;

public sealed class Neo4jUnitOfWork : BaseGraphDbUnitOfWork
{
    private readonly IAsyncSession _session;
    private readonly IAsyncTransaction _transaction;

    public Neo4jUnitOfWork(IDriver driver, ILogger<Neo4jUnitOfWork> logger) : base(logger)
    {
        _session = driver.AsyncSession();

        var ctx = new JoinableTaskContext();
		_transaction = ctx.Factory.Run(() => _session.BeginTransactionAsync());
    }

    public override async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters)
    {
        ThrowIfDisposed();

        var result = await _transaction.RunAsync(query, parameters);
        var records = await result.ToListAsync();

        var results = new List<T>(records.Count);
        foreach (var record in records)
        {
            results.Add(MapperCache<T>.MapFromRecord(new Neo4jRecord(record)));
        }

        return results;
    }
	public override ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<Core.IRecord, T>? mapper)
	{
		throw new NotImplementedException();
	}

	public override async ValueTask CommitAsync()
    {
        ThrowIfDisposed();

        await _transaction.CommitAsync();
        MarkCommitted();
        Logger.LogDebug("Neo4j UnitOfWork committed");
    }

    public override async ValueTask RollbackAsync()
    {
        ThrowIfDisposed();

        await _transaction.RollbackAsync();
		Logger.LogDebug("Neo4j UnitOfWork rolled back");
    }

	protected override async ValueTask InternalDisposeAsync()
    {
        await _transaction.DisposeAsync();
        await _session.DisposeAsync();
    }

    //private void ThrowIfDisposed()
    //{
    //    if (!IsDisposed)
    //        return;

    //    throw new ObjectDisposedException(nameof(Neo4jUnitOfWork));
    //} 
}