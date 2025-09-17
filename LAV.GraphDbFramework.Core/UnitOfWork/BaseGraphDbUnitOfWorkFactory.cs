using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public abstract  class BaseGraphDbUnitOfWorkFactory<T> : IGraphDbUnitOfWorkFactory
    where T : IGraphDbUnitOfWork
{
	protected readonly ILogger<T> Logger;

	protected BaseGraphDbUnitOfWorkFactory(ILogger<T> logger)
    {
        Logger = logger;
    }

    public abstract ValueTask<T> CreateAsync();
}