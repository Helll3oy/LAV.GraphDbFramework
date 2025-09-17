using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public interface IGraphDbUnitOfWork : IGraphDbQueryRunner, IAsyncDisposable
{
    ValueTask CommitAsync();
    ValueTask RollbackAsync();
    bool IsDisposed { get; }
    bool IsCommitted { get; }
}