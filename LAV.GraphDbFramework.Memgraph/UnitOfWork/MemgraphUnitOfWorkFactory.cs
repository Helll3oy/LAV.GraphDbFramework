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

public class MemgraphUnitOfWorkFactory : IGraphUnitOfWorkFactory
{
    private readonly IDriver _driver;
	private readonly ILoggerFactory _loggerFactory;

	private readonly ILogger<MemgraphUnitOfWork> _logger;

    public MemgraphUnitOfWorkFactory(IDriver driver, ILogger<MemgraphUnitOfWorkFactory> logger)
    {
        _driver = driver;
        //_loggerFactory = loggerFactory;
        _logger = logger;
    }

    public IGraphUnitOfWork Create()
    {
        return new MemgraphUnitOfWork(_driver, _logger);
    }

    public async ValueTask<IGraphUnitOfWork> CreateAsync()
    {
        return await Task.FromResult(Create());
    }
}