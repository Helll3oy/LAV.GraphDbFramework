using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public sealed class GraphDbTransactionException : GraphDbException
{
    public GraphDbTransactionException(string operation, Exception innerException)
        : base("Transaction operation failed", "TRANSACTION_ERROR", operation, innerException: innerException)
    {
    }

    private GraphDbTransactionException(string message, string errorCode, string? operation = null, string? query = null,
            object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
    }
}
