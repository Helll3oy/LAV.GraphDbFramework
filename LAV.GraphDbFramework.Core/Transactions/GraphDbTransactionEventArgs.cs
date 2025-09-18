using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Transactions;

public class GraphDbTransactionEventArgs : EventArgs
{
	public Guid TransactionId { get; }
	public DateTimeOffset EventTime { get; }

	public GraphDbTransactionEventArgs(Guid transactionId)
	{
		TransactionId = transactionId;
		EventTime = DateTimeOffset.UtcNow;
	}
}
