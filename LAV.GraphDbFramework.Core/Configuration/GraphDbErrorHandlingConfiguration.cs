using LAV.GraphDbFramework.Core.Exceptions;
using LAV.GraphDbFramework.Core.RetryPolicies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Configuration;

public static class GraphDbErrorHandlingConfiguration
{
	public static IServiceCollection AddGraphDbErrorHandling(this IServiceCollection services,
		IConfiguration configuration, string sectionName = "GraphDb:ErrorHandling")
	{
		services.Configure<GraphDbErrorHandlingOptions>(options => configuration.GetSection(sectionName).Bind(options));

        // Добавляем сервисы ProblemDetails
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var extendedProblemDetails = context.ProblemDetails as GraphDbExtendedProblemDetails;

                if (extendedProblemDetails == null)
                {
                    // Преобразуем стандартный ProblemDetails в расширенный
                    extendedProblemDetails = new GraphDbExtendedProblemDetails
                    {
                        Title = context.ProblemDetails.Title,
                        Status = context.ProblemDetails.Status,
                        Detail = context.ProblemDetails.Detail,
                        Type = context.ProblemDetails.Type,
                        Instance = context.ProblemDetails.Instance
                    };

                    // Копируем extensions
                    foreach (var extension in context.ProblemDetails.Extensions)
                    {
                        extendedProblemDetails.Extensions[extension.Key] = extension.Value;
                    }

                    context.ProblemDetails = extendedProblemDetails;
                }

                // Добавляем общую информацию
                extendedProblemDetails.Timestamp = DateTime.UtcNow;
                extendedProblemDetails.TraceId = context.HttpContext.TraceIdentifier;
                extendedProblemDetails.RequestId = Guid.NewGuid().ToString();

                // Добавляем идентификатор корреляции из заголовков
                if (context.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                {
                    extendedProblemDetails.CorrelationId = correlationId;
                }

                // Добавляем ссылку на документацию
                extendedProblemDetails.DocumentationUrl =
                    $"https://graphdbframework.com/docs/errors/{extendedProblemDetails.ErrorCode}";
            };
        });

        // Регистрируем глобальный обработчик исключений
        //services.AddTransient<IExceptionHandler, GraphDbGlobalExceptionHandler>();

        services.AddExceptionHandler<GraphDbGlobalExceptionHandler>();

        services.AddSingleton<IProblemDetailsService, GraphDbProblemDetailsService>();

        services.AddSingleton<IGraphDbRetryPolicy>(provider =>
		{
			var options = provider.GetRequiredService<IOptions<GraphDbErrorHandlingOptions>>().Value;
			var logger = provider.GetService<ILogger<GraphDbExponentialBackoffRetryPolicy>>();

			return new GraphDbExponentialBackoffRetryPolicy(
				options.MaxRetryAttempts,
				TimeSpan.FromMilliseconds(options.RetryInitialDelayMs),
				options.RetryBackoffFactor,
				logger);
		});

		return services;
	}

    public static IApplicationBuilder UseGraphDbErrorHandling(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<IOptions<GraphDbErrorHandlingOptions>>()?.Value
            ?? new GraphDbErrorHandlingOptions();

        if (options.EnableGlobalExceptionHandler)
        {
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                AllowStatusCode404Response = true,
                //ExceptionHandler = async context => 
                //    await context.RequestServices.GetRequiredService<GraphDbGlobalExceptionHandler>().InvokeAsync(context)
            });
        }

        app.UseStatusCodePages(async statusCodeContext =>
        {
            var problemDetailsService = statusCodeContext.HttpContext.RequestServices
                .GetService<IProblemDetailsService>();

            if (problemDetailsService != null)
            {
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = statusCodeContext.HttpContext,
                    ProblemDetails = new GraphDbExtendedProblemDetails
                    {
                        Status = statusCodeContext.HttpContext.Response.StatusCode,
                        Title = "An error occurred",
                        Type = $"https://httpstatuses.com/{statusCodeContext.HttpContext.Response.StatusCode}"
                    }
                });
            }
        });

        return app;
    }
}
