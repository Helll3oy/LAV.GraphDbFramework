using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LAV.GraphDbFramework.Core.Transactions;

public class GraphDbTransactionFailedEventArgs : GraphDbTransactionEventArgs
{
	public Exception Exception { get; }

	public GraphDbTransactionFailedEventArgs(Guid transactionId, Exception exception)
		: base(transactionId)
	{
		Exception = exception;
	}
}
