using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.RetryPolicies;

public interface IGraphDbRetryPolicy
{
	Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
	Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}