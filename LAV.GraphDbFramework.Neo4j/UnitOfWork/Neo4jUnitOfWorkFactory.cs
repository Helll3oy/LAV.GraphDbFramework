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

namespace LAV.GraphDbFramework.Neo4j.UnitOfWork;

public class Neo4jUnitOfWorkFactory : BaseGraphDbUnitOfWorkFactory<Neo4jUnitOfWork>
{
    private readonly IDriver _driver;

    public Neo4jUnitOfWorkFactory(IDriver driver, ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<Neo4jUnitOfWork>())
    {
        _driver = driver;
    }

    public override async ValueTask<Neo4jUnitOfWork> CreateAsync()
    {
        return await ValueTask.FromResult(new Neo4jUnitOfWork(_driver, Logger));
    }
}