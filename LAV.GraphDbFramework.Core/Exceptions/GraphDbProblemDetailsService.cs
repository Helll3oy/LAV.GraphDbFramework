using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbProblemDetailsService : IProblemDetailsService
{
    private readonly ILogger<GraphDbProblemDetailsService> _logger;

    public GraphDbProblemDetailsService(ILogger<GraphDbProblemDetailsService> logger = null)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        try
        {
            var httpContext = context.HttpContext;
            var problemDetails = context.ProblemDetails;

            // Устанавливаем статус код ответа
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            // Устанавливаем content-type
            httpContext.Response.ContentType = "application/problem+json";

            // Сериализуем ProblemDetails в JSON
            await httpContext.Response.WriteAsJsonAsync(problemDetails);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write problem details response");
            return false;
        }
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        _ = await TryWriteAsync(context);
    }
}
