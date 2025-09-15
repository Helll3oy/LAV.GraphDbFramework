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

public class Neo4jUnitOfWorkFactory : PooledObjectPolicy<IGraphUnitOfWork>, IGraphUnitOfWorkFactory
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jUnitOfWork> _logger;

    public Neo4jUnitOfWorkFactory(IDriver driver, ILogger<Neo4jUnitOfWork> logger)
    {
        _driver = driver;
        _logger = logger;
    }

    public override IGraphUnitOfWork Create()
    {
        return new Neo4jUnitOfWork(_driver, _logger);
    }

    public async Task<IGraphUnitOfWork> CreateAsync()
    {
        return await Task.FromResult(Create());
    }

    public override bool Return(IGraphUnitOfWork obj)
    {
        if (!obj.IsDisposed)
        {
            Task.WaitAll(Task.Run(async () => await obj.DisposeAsync()));
        }

        return true;
    }
}