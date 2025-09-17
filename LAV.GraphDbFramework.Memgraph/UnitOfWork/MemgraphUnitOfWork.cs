using LAV.GraphDbFramework.Core.Mapping;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Memgraph.UnitOfWork;

public sealed class MemgraphUnitOfWork : BaseGraphDbUnitOfWork<MemgraphRecord>
{
    private readonly IAsyncSession _session;
    private readonly Lazy<ConfiguredTaskAwaitable<IAsyncTransaction>> _transaction;

    private ConfiguredTaskAwaitable<IAsyncTransaction> Transaction => _transaction.Value;

	public MemgraphUnitOfWork(IDriver driver, ILogger<MemgraphUnitOfWork> logger) : base(logger)
    {
        _session = driver.AsyncSession();

		_transaction = new Lazy<ConfiguredTaskAwaitable<IAsyncTransaction>>(() =>
            _session.BeginTransactionAsync().ConfigureAwait(false));
    }

    public override async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters)
    {
        ThrowIfDisposed();

		var result = await(await Transaction).RunAsync(query, parameters);
        var records = await result.ToListAsync();

		var results = new List<T>(records.Count);
        foreach (var record in records)
        {
            results.Add(MapperCache<T>.MapFromRecord(new MemgraphRecord(record)));
        }

        return results;
    }

	public override async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<MemgraphRecord, T> mapper)
	{
		ThrowIfDisposed();

		var result = await(await Transaction).RunAsync(query, parameters);
		var records = await result.ToListAsync();

		var results = new List<T>(records.Count);
		foreach (var record in records)
		{
			results.Add(mapper(new MemgraphRecord(record)));
		}

		return results;
	}

	public override async ValueTask CommitAsync()
    {
        ThrowIfDisposed();

		await (await Transaction).CommitAsync();
        MarkCommitted();
        Logger.LogDebug("Memgraph UnitOfWork committed");
    }

    public override async ValueTask RollbackAsync()
    {
        ThrowIfDisposed();

		await (await Transaction).RollbackAsync();
        Logger.LogDebug("Memgraph UnitOfWork rolled back");
    }

    protected override async ValueTask InternalDisposeAsync()
    {
		await (await Transaction).DisposeAsync();
        await _session.DisposeAsync();
    }

	private void ThrowIfDisposed()
	{
		if (!IsDisposed)
			return;

		throw new ObjectDisposedException(nameof(MemgraphUnitOfWork));
	}
}