using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Memgraph.UnitOfWork;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Neo4j;

public sealed class Neo4jClient : IGraphClient
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jClient> _logger;
	private readonly ILoggerFactory _loggerFactory;
	private readonly Neo4jUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ConcurrentBag<IAsyncSession> _sessions = [];
    private bool _disposed;

    public Neo4jClient(string uri, string username, string password, ILoggerFactory loggerFactory)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password),
            o => o.WithMaxConnectionPoolSize(Environment.ProcessorCount * 2));
        _logger = loggerFactory.CreateLogger<Neo4jClient>();
        _loggerFactory = loggerFactory;
        _unitOfWorkFactory = new Neo4jUnitOfWorkFactory(_driver, loggerFactory);
    }

    public IGraphUnitOfWorkFactory UnitOfWorkFactory => _unitOfWorkFactory;

    public async ValueTask<T> ExecuteReadAsync<T>(Func<IQueryRunner, ValueTask<T>> operation)
    {
        var session = _driver.AsyncSession();
        _sessions.Add(session);

        try
        {
            return await session.ExecuteReadAsync(async tx =>
                await operation(new Neo4jQueryRunner(tx, _loggerFactory)));
        }
        finally
        {
            await session.CloseAsync();
            _sessions.TryTake(out _);
        }
    }

    public async ValueTask<T> ExecuteWriteAsync<T>(Func<IQueryRunner, ValueTask<T>> operation)
    {
        var session = _driver.AsyncSession();
        _sessions.Add(session);

        try
        {
            return await session.ExecuteWriteAsync(async tx =>
                await operation(new Neo4jQueryRunner(tx, _loggerFactory)));
        }
        finally
        {
            await session.CloseAsync();
            _sessions.TryTake(out _);
        }
    }

    public async ValueTask<IGraphUnitOfWork> BeginUnitOfWorkAsync()
    {
        return await _unitOfWorkFactory.CreateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

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