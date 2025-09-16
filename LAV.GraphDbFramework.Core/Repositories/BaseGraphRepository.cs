//using LAV.GraphDbFramework.Core.Pooling;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Repositories;

public abstract class BaseGraphRepository : IBaseGraphRepository, IAsyncDisposable
{
	protected IGraphUnitOfWork UnitOfWork { get; private set; }
	protected bool IsExternalUnitOfWork { get; private set; }

	protected BaseGraphRepository(/*IGraphClient client, */IServiceProvider provider)
	{
		var uowFactory = provider.GetRequiredService<IGraphUnitOfWorkFactory>();
		// По умолчанию создаем свой UnitOfWork
		UnitOfWork = uowFactory.Create();

			//client.BeginUnitOfWorkAsync().GetAwaiter().GetResult();
		IsExternalUnitOfWork = false;
	}

	protected BaseGraphRepository(IGraphUnitOfWork unitOfWork)
	{
		// Используем переданный извне UnitOfWork
		UnitOfWork = unitOfWork;
		IsExternalUnitOfWork = true;
	}

	public async ValueTask SaveChangesAsync()
	{
		if (UnitOfWork != null && !UnitOfWork.IsCommitted && !UnitOfWork.IsDisposed)
		{
			await UnitOfWork.CommitAsync();
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (UnitOfWork != null && !IsExternalUnitOfWork)
		{
			await UnitOfWork.DisposeAsync();
		}
	}
}

//public abstract class BaseGraphRepository1 : IAsyncDisposable, IBaseGraphRepository
//{
//	private readonly ObjectPool<IGraphUnitOfWork> _unitOfWorkPool;
//	private IGraphUnitOfWork? _currentUnitOfWork;
//	private bool _disposed;


//	protected BaseGraphRepository1(IGraphUnitOfWorkFactory unitOfWorkFactory)
//	{
//		//var uow = unitOfWorkFactory.Create();
//		_unitOfWorkPool = new DefaultObjectPool<IGraphUnitOfWork>(unitOfWorkFactory);
//	}

//	protected IGraphUnitOfWork GetUnitOfWork()
//	{
//		if (_currentUnitOfWork is not null)
//			return _currentUnitOfWork;

//		_currentUnitOfWork = _unitOfWorkPool.Get();
//		return _currentUnitOfWork;
//	}

//	protected void ReturnUnitOfWork()
//	{
//		if (_currentUnitOfWork is null) return;

//		_unitOfWorkPool.Return(_currentUnitOfWork);
//		_currentUnitOfWork = null;
//	}

//	public async ValueTask SaveChangesAsync()
//	{
//		if (_currentUnitOfWork is not null && !_currentUnitOfWork.IsCommitted)
//		{
//			await _currentUnitOfWork.CommitAsync();
//		}
//	}

//	public async ValueTask DisposeAsync()
//	{
//		if (_disposed) return;

//		if (_currentUnitOfWork is not null)
//		{
//			await _currentUnitOfWork.DisposeAsync();
//			ReturnUnitOfWork();
//		}

//		_disposed = true;
//	}
//}