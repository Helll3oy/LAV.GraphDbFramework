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

public class MemgraphUnitOfWorkFactory : BaseGraphDbUnitOfWorkFactory<MemgraphUnitOfWork>
{
    private readonly IDriver _driver;

    public MemgraphUnitOfWorkFactory(IDriver driver, ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<MemgraphUnitOfWork>())
    {
        _driver = driver;
    }

    public override async ValueTask<MemgraphUnitOfWork> CreateAsync()
    {
        return await ValueTask.FromResult(new MemgraphUnitOfWork(_driver, Logger));
    }
}