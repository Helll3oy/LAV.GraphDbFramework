using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public sealed class GraphDbConnectionException : GraphDbException
{
    public GraphDbConnectionException(string operation, Exception innerException)
        : base("Database connection failed", "CONNECTION_ERROR", operation, innerException: innerException)
    {
    }

    private GraphDbConnectionException(string message, string errorCode, string? operation = null, string? query = null,
            object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
    }
}