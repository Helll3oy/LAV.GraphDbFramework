using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LAV.GraphDbFramework.Core.Configuration;

public static class GraphDbErrorHandlingConfiguration
{
	public static IServiceCollection AddGraphErrorHandling(this IServiceCollection services,
		IConfiguration configuration, string sectionName = "GraphDb:ErrorHandling")
	{
		services.Configure<GraphDbErrorHandlingOptions>(configuration.GetSection(sectionName));

		services.AddSingleton<IRetryPolicy>(provider =>
		{
			var options = provider.GetRequiredService<IOptions<GraphDbErrorHandlingOptions>>().Value;
			var logger = provider.GetService<ILogger<ExponentialBackoffRetryPolicy>>();

			return new ExponentialBackoffRetryPolicy(
				options.MaxRetryAttempts,
				TimeSpan.FromMilliseconds(options.RetryInitialDelayMs),
				options.RetryBackoffFactor,
				logger);
		});

		services.AddTransient<GlobalExceptionHandler>();

		return services;
	}

	public static IApplicationBuilder UseGraphErrorHandling(this IApplicationBuilder app)
	{
		var options = app.ApplicationServices.GetService<IOptions<ErrorHandlingOptions>>().Value;

		if (options.EnableGlobalExceptionHandler)
		{
			app.UseMiddleware<GlobalExceptionHandler>();
		}

		return app;
	}
}
