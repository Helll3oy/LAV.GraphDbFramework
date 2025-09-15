using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Client;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddGraphDb(this IServiceCollection services,
        IConfiguration configuration, string sectionName = "GraphDb")
    {
        services.Configure<GraphDbOptions>(configuration.GetSection(sectionName));

        services.AddSingleton<IGraphClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphDbOptions>>().Value;
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            return GraphDbFactory.CreateClient(options, loggerFactory);
        });

        return services;
    }

    public static IServiceCollection AddGraphDb(this IServiceCollection services,
        Action<GraphDbOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<IGraphClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphDbOptions>>().Value;
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            return GraphDbFactory.CreateClient(options, loggerFactory);
        });

        return services;
    }
}
