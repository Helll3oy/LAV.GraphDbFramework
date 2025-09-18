using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Neo4j.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;

namespace LAV.GraphDbFramework.Neo4j;

public sealed class Neo4jClient
    : BaseGraphDbClient<Neo4jOptions, Neo4jQueryRunner, Neo4jUnitOfWork, Neo4jUnitOfWorkFactory>
{
    private readonly IServiceProvider _provider;
    private readonly IDriver _driver;

    private readonly ConcurrentBag<IAsyncSession> _sessions = [];

    public Neo4jClient(IServiceProvider provider, IOptionsMonitor<Neo4jOptions> monitor, ILoggerFactory loggerFactory)
        : base(monitor, loggerFactory.CreateLogger<Neo4jClient>())
    {
        _provider = provider;
        _driver = GraphDatabase.Driver(
            Options.Host,
            AuthTokens.Basic(Options.Username, Options.Password),
            o =>
            {
                o.WithMaxConnectionPoolSize(Environment.ProcessorCount * 2);
                o.WithConnectionTimeout(Options.ConnectionTimeout);
                o.WithMaxConnectionPoolSize(Options.MaxConnectionPoolSize);
                o.WithConnectionAcquisitionTimeout(Options.ConnectionAcquisitionTimeout);
                o.WithEncryptionLevel(Options.UseEncryption ? EncryptionLevel.Encrypted : EncryptionLevel.None);
            });

        UnitOfWorkFactory = new Neo4jUnitOfWorkFactory(_driver, loggerFactory);
    }

    public override Neo4jUnitOfWorkFactory UnitOfWorkFactory { get; }

    public override ValueTask<Neo4jUnitOfWork> BeginUnitOfWorkAsync()
    {
        throw new NotImplementedException();
    }

    public override async ValueTask<T> ExecuteReadAsync<T>(Func<Neo4jQueryRunner, ValueTask<T>> operation)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
            await operation(new Neo4jQueryRunner(tx, Logger)));
    }

    public override async ValueTask<T> ExecuteWriteAsync<T>(Func<Neo4jQueryRunner, ValueTask<T>> operation)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteWriteAsync(async tx =>
            await operation(new Neo4jQueryRunner(tx, Logger)));
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
