using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public sealed class GraphDbValidationException : GraphDbException
{
    public IReadOnlyList<GraphDbValidationError> ValidationErrors { get; }

    public GraphDbValidationException(IReadOnlyList<GraphDbValidationError> validationErrors)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        ValidationErrors = validationErrors;
    }

    private GraphDbValidationException(string message, string errorCode, string? operation = null, string? query = null,
            object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
        ValidationErrors = [];
    }
}
