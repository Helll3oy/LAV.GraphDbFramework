using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbTransactionException : GraphDbException
{
	public GraphDbTransactionException(string operation, Exception innerException)
		: base("Transaction operation failed", "TRANSACTION_ERROR", operation, innerException: innerException)
	{
	}
}
