using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbExtendedProblemDetails : ProblemDetails
{
    public string? ErrorCode { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? TraceId { get; set; }

    public string? RequestId { get; set; }

    public string? CorrelationId { get; set; }

    public List<GraphDbError>? Errors { get; set; }

    public string? DocumentationUrl { get; set; }

    public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

    // Конструкторы
    public GraphDbExtendedProblemDetails() { }

    public GraphDbExtendedProblemDetails(GraphDbException exception)
    {
        Title = exception.Message;
        Status = GetStatusCodeFromErrorCode(exception.ErrorCode);
        Detail = exception.DetailedMessage;
        Type = GetErrorTypeUri(exception.ErrorCode);
        ErrorCode = exception.ErrorCode;

        if (exception.Parameters != null)
        {
            Extensions["parameters"] = exception.Parameters;
        }

        if (!string.IsNullOrEmpty(exception.Query))
        {
            Extensions["query"] = exception.Query;
        }
    }

    private static int GetStatusCodeFromErrorCode(string errorCode)
    {
        return errorCode switch
        {
            "CONNECTION_ERROR" or "TRANSACTION_ERROR" => StatusCodes.Status503ServiceUnavailable,
            "QUERY_EXECUTION_ERROR" or "VALIDATION_ERROR" or "LINQ_TRANSLATOR_ERROR" => StatusCodes.Status400BadRequest,
            "MAPPING_ERROR" => StatusCodes.Status422UnprocessableEntity,
            "NOT_FOUND_ERROR" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetErrorTypeUri(string errorCode)
    {
        return errorCode switch
        {
            "CONNECTION_ERROR" => "https://graphdbframework.com/errors/connection-error",
            "TRANSACTION_ERROR" => "https://graphdbframework.com/errors/transaction-error",
            "QUERY_EXECUTION_ERROR" => "https://graphdbframework.com/errors/query-execution-error",
            "MAPPING_ERROR" => "https://graphdbframework.com/errors/mapping-error",
            "VALIDATION_ERROR" => "https://graphdbframework.com/errors/validation-error",
            "NOT_FOUND_ERROR" => "https://graphdbframework.com/errors/not-found-error",
			"LINQ_TRANSLATOR_ERROR" => "https://graphdbframework.com/errors/linq-translator-error",
			_ => "https://graphdbframework.com/errors/internal-server-error"
        };
    }
}