using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core.UnitOfWork;

public abstract class BaseGraphDbUnitOfWork<TRecord> : IGraphDbUnitOfWork<TRecord>
	where TRecord : IGraphDbRecord
{
	private bool _disposed;
	private bool _committed;

	protected readonly ILogger Logger;

	public bool IsDisposed => _disposed;
	public bool IsCommitted => _committed;

	protected BaseGraphDbUnitOfWork(ILogger logger)
	{
		Logger = logger;
	}

	public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);
	//public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<IRecord, T> mapper)
	//{
	//	return await InternalRunAsync<T>(query, parameters, (record) => mapper!(record));
	//}

	public abstract ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<TRecord, T> mapper);

	public abstract ValueTask CommitAsync();
	public abstract ValueTask RollbackAsync();

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		if (!_committed)
		{
			try
			{
				await RollbackAsync();
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, "Error during rollback on dispose");
			}
		}

		await InternalDisposeAsync();

		_disposed = true;
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask InternalDisposeAsync() => ValueTask.CompletedTask;

	protected void MarkCommitted() => _committed = true;

	
}