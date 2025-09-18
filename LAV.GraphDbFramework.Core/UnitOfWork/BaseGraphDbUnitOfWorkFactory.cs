using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public abstract class BaseGraphDbUnitOfWorkFactory<TGraphDbUnitOfWork> : IGraphDbUnitOfWorkFactory
    where TGraphDbUnitOfWork : IGraphDbUnitOfWork
{
    protected readonly ILogger<TGraphDbUnitOfWork> Logger;

    protected BaseGraphDbUnitOfWorkFactory(ILogger<TGraphDbUnitOfWork> logger)
    {
        Logger = logger;
    }

    public abstract ValueTask<TGraphDbUnitOfWork> CreateAsync();

    async ValueTask<IGraphDbUnitOfWork> IGraphDbUnitOfWorkFactory.CreateAsync()
    {
        return await CreateAsync();
    }
}