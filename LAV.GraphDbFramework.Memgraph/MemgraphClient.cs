using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Memgraph.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;

namespace LAV.GraphDbFramework.Memgraph;

public sealed class MemgraphClient : BaseGraphDbClient
{
    private readonly IDriver _driver;

	private readonly MemgraphUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ConcurrentBag<IAsyncSession> _sessions = [];
    private bool _disposed;

	public MemgraphClient(string host, string username, string password, ILogger<MemgraphClient> logger) : base(logger)
    {
        _driver = GraphDatabase.Driver(host, AuthTokens.Basic(username, password),
            o => o.WithMaxConnectionPoolSize(Environment.ProcessorCount * 2));

        _unitOfWorkFactory = new MemgraphUnitOfWorkFactory(_driver, Logger);
    }

    public override IGraphUnitOfWorkFactory UnitOfWorkFactory => _unitOfWorkFactory;

    public override async ValueTask<T> ExecuteReadAsync<T>(Func<IQueryRunner, ValueTask<T>> operation)
    {
        var session = _driver.AsyncSession();
        _sessions.Add(session);

        try
        {
            return await session.ExecuteReadAsync(async tx =>
                await operation(new MemgraphQueryRunner(tx, Logger)));
        }
        finally
        {
            await session.CloseAsync();
            _sessions.TryTake(out _);
        }
    }

    public override async ValueTask<T> ExecuteWriteAsync<T>(Func<IQueryRunner, ValueTask<T>> operation)
    {
        var session = _driver.AsyncSession();
        _sessions.Add(session);

        try
        {
            return await session.ExecuteWriteAsync(async tx =>
                await operation(new MemgraphQueryRunner(tx, Logger)));
        }
        finally
        {
            await session.CloseAsync();
            _sessions.TryTake(out _);
        }
    }

    protected override async ValueTask InternalDisposeAsync()
    {
        // Закрываем все активные сессии
        foreach (var session in _sessions)
        {
            try
            {
                await session.CloseAsync();
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }
        }

        await _driver.DisposeAsync();
        _sessions.Clear();
    }
}
