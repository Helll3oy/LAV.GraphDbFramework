using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbGlobalExceptionHandler : IExceptionHandler
{
	//private readonly RequestDelegate _next;
	private readonly ILogger<GraphDbGlobalExceptionHandler> _logger;
	private readonly IHostEnvironment _environment;

    private readonly IProblemDetailsService _problemDetailsService;

    public GraphDbGlobalExceptionHandler(
        //RequestDelegate next, 
		ILogger<GraphDbGlobalExceptionHandler> logger, 
        IHostEnvironment environment,
        IProblemDetailsService problemDetailsService)
	{
		//_next = next;
		_logger = logger;
		_environment = environment;
        _problemDetailsService = problemDetailsService;
    }

    //public async ValueTask InvokeAsync(HttpContext context)
    //{
    //    try
    //    {
    //        await _next(context);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unhandled exception occurred");
    //        await TryHandleAsync(context, ex, CancellationToken.None);
    //    }
    //}

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        //context.Response.ContentType = "application/json";
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = CreateProblemDetails(httpContext, exception);

        // Логируем ошибку для последующего анализа
        LogException(exception, problemDetails);

        // Используем стандартный сервис ProblemDetails если доступен
        if (_problemDetailsService != null)
        {
            return await TryHandleWithProblemDetailsServiceAsync(httpContext, problemDetails);
        }
        else
        {
            // Fallback: самостоятельно записываем ответ
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
        }

        return true;

        /*
        var problemDetails = new ProblemDetails
		{
			Instance = context.Request.Path,
			Title = "An error occurred while processing your request.",
			Status = StatusCodes.Status500InternalServerError,
			Detail = _environment.IsDevelopment() ? exception.ToString() : null
		};

		switch (exception)
		{
			case GraphDbValidationException validationEx:
				context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
				problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
				problemDetails.Title = "Validation failed";
				problemDetails.Extensions["errors"] = validationEx.ValidationErrors;
				break;

			case GraphDbConnectionException:
				context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
				problemDetails.Status = StatusCodes.Status503ServiceUnavailable;
				problemDetails.Title = "Service unavailable";
				break;

			case GraphDbQueryException queryEx:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Title = "Invalid query";
				problemDetails.Extensions["query"] = queryEx.Query;
				break;

            case GraphDbException graphEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Graph database error";
                problemDetails.Extensions["errorCode"] = graphEx.ErrorCode;
                problemDetails.Extensions["operation"] = graphEx.Operation;
                break;

            default:
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				problemDetails.Status = StatusCodes.Status500InternalServerError;
				problemDetails.Title = "Internal server error";
				break;
		}

		if (_environment.IsDevelopment())
		{
			problemDetails.Extensions["traceId"] = context.TraceIdentifier;
			problemDetails.Extensions["exception"] = exception.ToString();
		}

		await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        */
    }

    private GraphDbExtendedProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            GraphDbValidationException validationEx => CreateValidationProblemDetails(validationEx),
            GraphDbTransactionException transEx => new GraphDbExtendedProblemDetails(transEx),
            GraphDbMappingException mapEx => new GraphDbExtendedProblemDetails(mapEx),
            GraphDbQueryException queryEx => new GraphDbExtendedProblemDetails(queryEx),
            GraphDbConnectionException connEx => CreateConnectionProblemDetails(connEx),
            GraphDbException graphEx => new GraphDbExtendedProblemDetails(graphEx),

            _ => new GraphDbExtendedProblemDetails()
        };

        // Заполняем общие поля
        problemDetails.Instance = context.Request.Path;
        problemDetails.TraceId = context.TraceIdentifier;
        problemDetails.RequestId = Guid.NewGuid().ToString();

        // Добавляем диагностическую информацию в development среде
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["debug"] = new
            {
                exception.StackTrace,
                exception.Source,
                InnerException = exception.InnerException?.Message
            };
        }

        // Добавляем информацию для трассировки
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
        problemDetails.Extensions["spanId"] = Activity.Current?.SpanId.ToString();

        return problemDetails;
    }

    private GraphDbExtendedProblemDetails CreateValidationProblemDetails(GraphDbValidationException exception)
    {
        var problemDetails = new GraphDbExtendedProblemDetails
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred",
            Type = "https://graphdbframework.com/errors/validation-error",
            ErrorCode = "VALIDATION_ERROR"
        };

        if (exception.ValidationErrors != null)
        {
            problemDetails.Errors = exception.ValidationErrors
                .Select(e => new GraphDbError(e.ErrorCode, e.ErrorMessage, e.PropertyName, e.AttemptedValue))
                .ToList();
        }

        return problemDetails;
    }

    private GraphDbExtendedProblemDetails CreateConnectionProblemDetails(GraphDbConnectionException exception)
    {
        var problemDetails = new GraphDbExtendedProblemDetails
        {
            Title = "Connection problem",
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = exception.DetailedMessage,
            Type = "https://regress.komifoms.ru/errors/connection-error",
            ErrorCode = exception.ErrorCode
        };

        return problemDetails;
    }

    private async Task<bool> TryHandleWithProblemDetailsServiceAsync(HttpContext context, GraphDbExtendedProblemDetails problemDetails)
    {
        try
        {
            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails,
                Exception = context.Features.Get<IExceptionHandlerFeature>()?.Error
            };

            return await _problemDetailsService.TryWriteAsync(problemDetailsContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write problem details response");
            return false;
        }
    }

    private void LogException(Exception exception, GraphDbExtendedProblemDetails problemDetails)
    {
        var logLevel = exception is GraphDbException ? LogLevel.Warning : LogLevel.Error;

        _logger.Log(logLevel, exception,
            "Exception handled: {ErrorCode}, RequestId: {RequestId}, TraceId: {TraceId}",
            problemDetails.ErrorCode, problemDetails.RequestId, problemDetails.TraceId);
    }

    
}
