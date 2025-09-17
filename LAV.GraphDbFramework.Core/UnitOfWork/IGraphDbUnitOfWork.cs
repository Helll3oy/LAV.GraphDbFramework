using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public interface IGraphDbUnitOfWork : IGraphDbUnitOfWork<IGraphDbRecord>
{

}

public interface IGraphDbUnitOfWork<TGraphDbRecord> : IGraphDbQueryRunner<TGraphDbRecord>
    where TGraphDbRecord : IGraphDbRecord
{
    ValueTask CommitAsync();
    ValueTask RollbackAsync();
    bool IsDisposed { get; }
    bool IsCommitted { get; }
}