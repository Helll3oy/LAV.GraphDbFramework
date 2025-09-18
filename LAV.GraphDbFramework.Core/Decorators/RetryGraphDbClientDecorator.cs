using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Exceptions;
using LAV.GraphDbFramework.Core.RetryPolicies;
using LAV.GraphDbFramework.Core.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core.Decorators;

public class RetryGraphDbClientDecorator : IGraphDbClient
{
	private readonly IGraphDbClient _innerClient;
	private readonly IGraphDbRetryPolicy _retryPolicy;
	private readonly ILogger<RetryGraphDbClientDecorator> _logger;

	public RetryGraphDbClientDecorator(IGraphDbClient innerClient, IGraphDbRetryPolicy retryPolicy,
		ILogger<RetryGraphDbClientDecorator> logger)
	{
		_innerClient = innerClient;
		_retryPolicy = retryPolicy;
		_logger = logger;
	}

	public async ValueTask<T> ExecuteReadAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		return await _retryPolicy.ExecuteAsync(async ct =>
		{
			try
			{
				return await _innerClient.ExecuteReadAsync(operation).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Read operation failed");
				throw new GraphDbException("Read operation failed", "READ_OPERATION_ERROR", innerException: ex);
			}
		});
	}

	public async ValueTask<T> ExecuteWriteAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		return await _retryPolicy.ExecuteAsync(async ct =>
		{
			try
			{
				return await _innerClient.ExecuteWriteAsync(operation).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Write operation failed");
				throw new GraphDbException("Write operation failed", "WRITE_OPERATION_ERROR", innerException: ex);
			}
		});
	}

	public async ValueTask<IGraphDbUnitOfWork> BeginUnitOfWorkAsync()
	{
		return await _retryPolicy.ExecuteAsync(async ct =>
		{
			try
			{
				return await _innerClient.BeginUnitOfWorkAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to begin unit of work");
				throw new GraphDbTransactionException("BeginUnitOfWork", ex);
			}
		});
	}

	public IGraphDbUnitOfWorkFactory UnitOfWorkFactory => _innerClient.UnitOfWorkFactory;

	public async ValueTask DisposeAsync()
	{
		await _innerClient.DisposeAsync().ConfigureAwait(false);
	}
}