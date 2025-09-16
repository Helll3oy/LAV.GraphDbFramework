using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public interface IGraphUnitOfWorkFactory// : IPooledObjectPolicy<IGraphUnitOfWork>
{
    ValueTask<IGraphUnitOfWork> CreateAsync();
}
