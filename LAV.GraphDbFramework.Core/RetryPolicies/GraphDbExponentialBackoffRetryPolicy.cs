using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core.RetryPolicies;

internal class GraphDbExponentialBackoffRetryPolicy : IGraphDbRetryPolicy
{
	private readonly int _maxRetryAttempts;
	private readonly TimeSpan _initialDelay;
	private readonly double _backoffFactor;
	private readonly ILogger<GraphDbExponentialBackoffRetryPolicy> _logger;
	private readonly HashSet<Type> _retryableExceptions;

	public GraphDbExponentialBackoffRetryPolicy(int maxRetryAttempts = 5, TimeSpan? initialDelay = null,
		double backoffFactor = 2.0, ILogger<GraphDbExponentialBackoffRetryPolicy> logger = null)
	{
		_maxRetryAttempts = maxRetryAttempts;
		_initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
		_backoffFactor = backoffFactor;
		_logger = logger;

		_retryableExceptions = new HashSet<Type>
		{
			typeof(GraphDbConnectionException),
			typeof(TimeoutException),
			typeof(SocketException),
			typeof(IOException)
		};
	}

	public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
	{
		int attempt = 0;
		TimeSpan delay = _initialDelay;
		List<Exception> exceptions = new List<Exception>();

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				return await operation(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsRetryableException(ex) && attempt < _maxRetryAttempts)
			{
				exceptions.Add(ex);
				attempt++;

				_logger?.LogWarning(ex, "Operation failed, retrying in {DelayMs}ms (attempt {Attempt}/{MaxAttempts})",
					delay.TotalMilliseconds, attempt, _maxRetryAttempts);

				await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
				delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffFactor);
			}
			catch (Exception ex)
			{
				if (exceptions.Count > 0)
				{
					throw new AggregateException("Operation failed after multiple retry attempts", exceptions);
				}
				throw;
			}
		}
	}

	public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
	{
		await ExecuteAsync<object>(async ct =>
		{
			await operation(ct).ConfigureAwait(false);
			return null;
		}, cancellationToken).ConfigureAwait(false);
	}

	private bool IsRetryableException(Exception exception)
	{
		return _retryableExceptions.Any(t => t.IsInstanceOfType(exception)) ||
			   exception is GraphDbException gex && gex.ErrorCode == "CONNECTION_ERROR";
	}

	public void AddRetryableException<T>() where T : Exception
	{
		_retryableExceptions.Add(typeof(T));
	}

	public void RemoveRetryableException<T>() where T : Exception
	{
		_retryableExceptions.Remove(typeof(T));
	}
}
