using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Exceptions;

namespace LAV.GraphDbFramework.Linq;

public sealed class GraphDbLinqException : GraphDbException
{
    public GraphDbLinqException(string query, object parameters, Exception innerException)
        : base("Query execution failed", "LINQ_TRANSLATOR_ERROR", "ExecuteQuery", query, parameters, innerException)
    {
    }

    private GraphDbLinqException(string message, string errorCode, string? operation = null,
            string? query = null, object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
    }
}