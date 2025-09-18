using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Memgraph.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;

namespace LAV.GraphDbFramework.Memgraph;

public sealed class MemgraphClient
    : BaseGraphDbClient<MemgraphOptions, MemgraphQueryRunner, MemgraphUnitOfWork, MemgraphUnitOfWorkFactory>
{
    private readonly IServiceProvider _provider;
    private readonly IDriver _driver;

    private readonly ConcurrentBag<IAsyncSession> _sessions = [];

    public MemgraphClient(IServiceProvider provider, IOptionsMonitor<MemgraphOptions> monitor, ILoggerFactory loggerFactory)
        : base(monitor, loggerFactory.CreateLogger<MemgraphClient>())
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

        UnitOfWorkFactory = new MemgraphUnitOfWorkFactory(_driver, loggerFactory);
    }

    public override MemgraphUnitOfWorkFactory UnitOfWorkFactory { get; }

    public async override ValueTask<MemgraphUnitOfWork> BeginUnitOfWorkAsync()
    {
        return await UnitOfWorkFactory.CreateAsync().ConfigureAwait(false);
    }

    //public MemgraphClient(string host, string username, string password, ILoggerFactory loggerFactory) 
    //       : base(loggerFactory.CreateLogger<MemgraphClient>())
    //   {
    //	_driver = GraphDatabase.Driver(host, AuthTokens.Basic(username, password),
    //           o => o.WithMaxConnectionPoolSize(Environment.ProcessorCount * 2));

    //       //_unitOfWorkFactory = new MemgraphUnitOfWorkFactory(_driver, loggerFactory.CreateLogger<MemgraphClient>());
    //   }

    //public override IGraphDbUnitOfWorkFactory UnitOfWorkFactory => _unitOfWorkFactory;

    public override async ValueTask<T> ExecuteReadAsync<T>(Func<MemgraphQueryRunner, ValueTask<T>> operation)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
            await operation(new MemgraphQueryRunner(tx, Logger)));

        //var session = _driver.AsyncSession();
        //_sessions.Add(session);

        //try
        //{
        //    return await session.ExecuteReadAsync(async tx =>
        //        await operation(new MemgraphQueryRunner(tx, Logger)));
        //}
        //finally
        //{
        //    await session.CloseAsync();
        //    _sessions.TryTake(out _);
        //}
    }

    public override async ValueTask<T> ExecuteWriteAsync<T>(Func<MemgraphQueryRunner, ValueTask<T>> operation)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteWriteAsync(async tx =>
            await operation(new MemgraphQueryRunner(tx, Logger)));

        //var session = _driver.AsyncSession();
        //_sessions.Add(session);

        //try
        //{
        //    return await session.ExecuteWriteAsync(async tx =>
        //        await operation(new MemgraphQueryRunner(tx, Logger)));
        //}
        //finally
        //{
        //    await session.CloseAsync();
        //    _sessions.TryTake(out _);
        //}
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
