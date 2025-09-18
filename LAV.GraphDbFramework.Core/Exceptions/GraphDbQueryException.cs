using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public sealed class GraphDbQueryException : GraphDbException
{
    public GraphDbQueryException(string query, object parameters, Exception innerException)
        : base("Query execution failed", "QUERY_EXECUTION_ERROR", "ExecuteQuery", query, parameters, innerException)
    {
    }

    private GraphDbQueryException(string message, string errorCode, string? operation = null,
            string? query = null, object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
    }
}