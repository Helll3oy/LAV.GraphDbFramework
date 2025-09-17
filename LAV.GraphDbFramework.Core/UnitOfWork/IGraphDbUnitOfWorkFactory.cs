using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public interface IGraphDbUnitOfWorkFactory :
	IGraphDbUnitOfWorkFactory<IGraphDbUnitOfWork, IGraphDbRecord>
{
}

public interface IGraphDbUnitOfWorkFactory<TGraphDbUnitOfWork, TGraphDbRecord>
	where TGraphDbRecord : IGraphDbRecord
	where TGraphDbUnitOfWork : IGraphDbUnitOfWork<TGraphDbRecord>
	// : IPooledObjectPolicy<IGraphUnitOfWork>
{
    ValueTask<TGraphDbUnitOfWork> CreateAsync();
}
