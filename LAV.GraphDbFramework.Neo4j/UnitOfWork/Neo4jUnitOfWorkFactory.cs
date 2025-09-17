using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Neo4j.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Memgraph.UnitOfWork;

public class Neo4jUnitOfWorkFactory : PooledObjectPolicy<IGraphDbUnitOfWork>, IGraphUnitOfWorkFactory
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jUnitOfWorkFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;

	public Neo4jUnitOfWorkFactory(IDriver driver, ILoggerFactory loggerFactory)
    {
        _driver = driver;
        _loggerFactory = loggerFactory;
		_logger = loggerFactory.CreateLogger<Neo4jUnitOfWorkFactory>();
    }

    public override IGraphDbUnitOfWork Create()
    {
        return new Neo4jUnitOfWork(_driver, _loggerFactory);
    }

    public async Task<IGraphDbUnitOfWork> CreateAsync()
    {
        return await Task.FromResult(Create());
    }

    public override bool Return(IGraphDbUnitOfWork obj)
    {
        if (!obj.IsDisposed)
        {
            Task.WaitAll(Task.Run(async () => await obj.DisposeAsync()));
        }

        return true;
    }
}