using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Transactions;

public interface IGraphDbTransaction : IAsyncDisposable
{
	GraphDbTransactionStatus Status { get; }
	Guid TransactionId { get; }
	DateTimeOffset StartTime { get; }
	TimeSpan Timeout { get; }

	Task CommitAsync();
	Task RollbackAsync();
	Task SavepointAsync(string name);
	Task RollbackToSavepointAsync(string name);

	event EventHandler<GraphDbTransactionEventArgs> Committed;
	event EventHandler<GraphDbTransactionEventArgs> RolledBack;
	event EventHandler<GraphDbTransactionFailedEventArgs> Failed;
}
