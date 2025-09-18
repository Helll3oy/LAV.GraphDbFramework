using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbGlobalExceptionHandler
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GraphDbGlobalExceptionHandler> _logger;
	private readonly IHostEnvironment _environment;

	public GraphDbGlobalExceptionHandler(RequestDelegate next, 
		ILogger<GraphDbGlobalExceptionHandler> logger, IHostEnvironment environment)
	{
		_next = next;
		_logger = logger;
		_environment = environment;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var problemDetails = new ProblemDetails
		{
			Instance = context.Request.Path,
			Title = "An error occurred while processing your request.",
			Status = StatusCodes.Status500InternalServerError,
			Detail = _environment.IsDevelopment() ? exception.ToString() : null
		};

		switch (exception)
		{
			case GraphDbException graphEx:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Title = "Graph database error";
				problemDetails.Extensions["errorCode"] = graphEx.ErrorCode;
				problemDetails.Extensions["operation"] = graphEx.Operation;
				break;

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
	}
}
