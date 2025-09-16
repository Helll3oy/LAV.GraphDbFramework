using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Memgraph.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Memgraph.UnitOfWork;

public class MemgraphUnitOfWorkFactory : PooledObjectPolicy<IGraphUnitOfWork>, IGraphUnitOfWorkFactory
{
    private readonly IDriver _driver;
	private readonly ILoggerFactory _loggerFactory;

	private readonly ILogger<MemgraphUnitOfWork> _logger;

    public MemgraphUnitOfWorkFactory(IDriver driver, ILoggerFactory loggerFactory)
    {
        _driver = driver;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<MemgraphUnitOfWork>();
    }

    public override IGraphUnitOfWork Create()
    {
        return new MemgraphUnitOfWork(_driver, _loggerFactory);
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