//using LAV.GraphDbFramework.Core.Pooling;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Repositories;

public abstract class BaseGraphRepository : IAsyncDisposable
{
    private readonly ObjectPool<IGraphUnitOfWork> _unitOfWorkPool;
    private IGraphUnitOfWork? _currentUnitOfWork;
    private bool _disposed;

    protected BaseGraphRepository(IGraphUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkPool = ObjectPool.Create<IGraphUnitOfWork>((PooledObjectPolicy<IGraphUnitOfWork>)unitOfWorkFactory);
    }

    protected IGraphUnitOfWork GetUnitOfWork()
    {
        if (_currentUnitOfWork is not null)
            return _currentUnitOfWork;

        _currentUnitOfWork = _unitOfWorkPool.Get();
        return _currentUnitOfWork;
    }

    protected void ReturnUnitOfWork()
    {
        if (_currentUnitOfWork is null) return;

        _unitOfWorkPool.Return(_currentUnitOfWork);
        _currentUnitOfWork = null;
    }

    public async ValueTask SaveChangesAsync()
    {
        if (_currentUnitOfWork is not null && !_currentUnitOfWork.IsCommitted)
        {
            await _currentUnitOfWork.CommitAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_currentUnitOfWork is not null)
        {
            await _currentUnitOfWork.DisposeAsync();
            ReturnUnitOfWork();
        }

        _disposed = true;
    }
}