using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.RetryPolicies;

public class NoRetryPolicy : IGraphDbRetryPolicy
{
	public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
	{
		return await operation(cancellationToken).ConfigureAwait(false);
	}

	public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
	{
		await operation(cancellationToken).ConfigureAwait(false);
	}
}