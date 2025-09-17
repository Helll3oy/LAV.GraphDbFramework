using LAV.GraphDbFramework.Core;
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

namespace LAV.GraphDbFramework.Neo4j.UnitOfWork;

public sealed class Neo4jUnitOfWork : BaseGraphDbUnitOfWork<Neo4jRecord>
{
    private readonly IAsyncSession _session;
    private readonly Lazy<ConfiguredTaskAwaitable<IAsyncTransaction>> _transaction;

    private ConfiguredTaskAwaitable<IAsyncTransaction> Transaction => _transaction.Value;

    public Neo4jUnitOfWork(IDriver driver, ILogger<Neo4jUnitOfWork> logger) : base(logger)
    {
        _session = driver.AsyncSession();

        _transaction = new Lazy<ConfiguredTaskAwaitable<IAsyncTransaction>>(() =>
            _session.BeginTransactionAsync().ConfigureAwait(false));
    }

    public override async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters)
    {
        ThrowIfDisposed();

        var transaction = await Transaction;
        var result = await transaction.RunAsync(query, parameters);
        var records = await result.ToListAsync();

        var results = new List<T>(records.Count);
        foreach (var record in records)
        {
            results.Add(MapperCache<T>.MapFromRecord(new Neo4jRecord(record)));
        }

        return results;
    }

    public override async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<Neo4jRecord, T> mapper)
    {
        ThrowIfDisposed();

        var transaction = await Transaction;
        var result = await transaction.RunAsync(query, parameters);
        var records = await result.ToListAsync();

        var results = new List<T>(records.Count);
        foreach (var record in records)
        {
            results.Add(mapper(new Neo4jRecord(record)));
        }

        return results;
    }

    public override async ValueTask CommitAsync()
    {
        ThrowIfDisposed();

        var transaction = await Transaction;
        await transaction.CommitAsync();
        MarkCommitted();
        Logger.LogDebug("Neo4j UnitOfWork committed");
    }

    public override async ValueTask RollbackAsync()
    {
        ThrowIfDisposed();

        var transaction = await Transaction;
        await transaction.RollbackAsync();
        Logger.LogDebug("Neo4j UnitOfWork rolled back");
    }

    protected override async ValueTask InternalDisposeAsync()
    {
        var transaction = await Transaction;
        await transaction.DisposeAsync();
        await _session.DisposeAsync();
    }
}