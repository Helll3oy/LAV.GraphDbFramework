using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
//using Microsoft.Extensions.Options.ConfigurationExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Memgraph;
//using LAV.GraphDbFramework.Neo4j;
using LAV.GraphDbFramework.Core.Transactions;
using LAV.GraphDbFramework.Linq;
using LAV.GraphDbFramework.QueryLanguage;

namespace LAV.GraphDbFramework.Client;

public static class DependencyInjectionExtensions
{
    //private static string[] _graphDbTypes = ["Neo4j", "Memgraph"];

    private static IServiceCollection AddDefaults(IServiceCollection services)
    {
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        // Регистрируем пулы 
        services.AddSingleton(provider =>
        {
            var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
            return poolProvider.Create<Dictionary<string, object>>(new ParameterPoolPolicy());
        });

        services.AddSingleton(provider =>
        {
            var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
            return poolProvider.Create<HashSet<string>>(new HashSetPoolPolicy());
        });

		// Менеджер транзакций
		services.AddScoped<GraphDbDistributedTransactionManager>();

		// Регистрируем спецификации как транзитные (transient)
		services.AddTransient<NodeSpecification>();
        services.AddTransient<RelationshipSpecification>();

		// Универсальный клиент
		services.AddScoped<MultiLanguageGraphDbClient>(provider =>
		{
			var innerClient = provider.GetRequiredService<IGraphDbClient>();
			var providerFactory = provider.GetRequiredService<QueryLanguageProviderFactory>();
			var logger = provider.GetService<ILogger<MultiLanguageGraphDbClient>>();

			return new MultiLanguageGraphDbClient(innerClient, providerFactory, logger!);
		});

		//services.AddSingleton<IGraphDbClient>(provider =>
		//{
		//    var clientFactory = provider.GetRequiredService<IGraphDbClientFactory>();
		//    return clientFactory.Create();

		//    //var options = provider.GetRequiredService<IOptions<GraphDbOptions>>().Value;
		//    //var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
		//    //return GraphDbClientFactory.CreateClient(options, loggerFactory);
		//});

		//// Регистрируем фабрику Unit of Work
		//services.AddScoped<IGraphDbUnitOfWorkFactory>(provider =>
		//{
		//    var client = provider.GetRequiredService<IGraphDbClient>();
		//    return client.UnitOfWorkFactory;
		//});

		//services.AddSingleton<ObjectPool<IGraphUnitOfWork>>(provider =>
		//{
		//	var factory = provider.GetRequiredService<IGraphUnitOfWorkFactory>();
		//	var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
		//	return poolProvider.Create<IGraphUnitOfWork>(factory);
		//});

		//// Автоматическая регистрация репозиториев
		//services.Scan(scan => scan
		//	.FromAssembliesOf(typeof(IUserRepository))
		//	.AddClasses(classes => classes.AssignableToAny(
		//		new[] { typeof(BaseGraphRepository) }))
		//	.AsImplementedInterfaces()
		//	.WithScopedLifetime());

		return services;
    }

    public static IServiceCollection AddGraphDb(this IServiceCollection services,
        IConfiguration configuration, string sectionName = "GraphDb")
    {
        services.Configure<GraphDbOptions>(options => configuration.GetSection(sectionName).Bind(options));

        return AddDefaults(services);
    }

    public static IServiceCollection AddGraphDb(this IServiceCollection services,
        Action<GraphDbOptions> configureOptions)
    {
        services.Configure(configureOptions);

        return AddDefaults(services);
    }

    public static IServiceCollection AddMemgraphGraphDbClient(this IServiceCollection services,
        IConfiguration configuration, string sectionName = "GraphDb:Memgraph")
    {
        services.AddOptions<MemgraphOptions>()
            .Configure(options => configuration.GetRequiredSection(sectionName).Bind(options))
            .ValidateOnStart();

        services.AddSingleton<IGraphDbClient, MemgraphClient>();

        //services.AddTransient<IGraphDbQueryRunner, MemgraphQueryRunner>();

        //services.Configure<MemgraphOptions>(options => configuration.GetSection(sectionName).Bind(options));

        return AddDefaults(services);
    }

	//public static IServiceCollection AddNeo4jGraphDbClient(this IServiceCollection services,
	//    IConfiguration configuration, string sectionName = "GraphDb:Neo4j")
	//{
	//    services.AddOptions<Neo4jOptions>()
	//        .Configure(options => configuration.GetRequiredSection(sectionName).Bind(options))
	//        .ValidateOnStart();

	//    services.AddSingleton<Neo4jClient>();

	//    return AddDefaults(services);
	//}

	public static IServiceCollection AddGraphDbLinqSupport(this IServiceCollection services)
	{
		services.AddScoped<IGraphDbQueryProvider, CypherQueryProvider>();
		services.AddScoped<CypherExpressionTranslator>();

		// LINQ-провайдер
		//services.AddSingleton<CypherExpressionTranslator>();
		//services.AddScoped<IGraphDbQueryProvider>(provider =>
		//{
		//	var graphClient = provider.GetRequiredService<IGraphDbClient>();
		//	var logger = provider.GetService<ILogger<CypherQueryProvider>>();
		//	return new CypherQueryProvider(graphClient, logger!);
		//});

		return services;
	}

	public static IServiceCollection AddQueryLanguageProvider<TProvider>(this IServiceCollection services,
		QueryLanguageType language, Func<IServiceProvider, IGraphDbClient, TProvider> factory)
		where TProvider : IQueryLanguageProvider
	{
		services.AddSingleton(provider =>
		{
			var providerFactory = provider.GetRequiredService<QueryLanguageProviderFactory>();
			//var graphClient = provider.GetRequiredService<IGraphDbClient>();

			providerFactory.RegisterProvider(language, (client) => factory(provider, client));
			return providerFactory;
		});

		return services;
	}

	private sealed class ParameterPoolPolicy : IPooledObjectPolicy<Dictionary<string, object>>
    {
        public Dictionary<string, object> Create() => new(16);

        public bool Return(Dictionary<string, object> obj)
        {
            obj.Clear();
            return true;
        }
    }

    private class SpecificationBuilderPoolPolicy<T> : IPooledObjectPolicy<SpecificationBuilder.PooledSpecificationBuilder<T>>
        where T : class
    {
        public SpecificationBuilder.PooledSpecificationBuilder<T> Create()
        {
            return SpecificationBuilder.Create<T>();
        }

        public bool Return(SpecificationBuilder.PooledSpecificationBuilder<T> obj)
        {
            obj.Dispose();
            return true;
        }
    }

    private class HashSetPoolPolicy : IPooledObjectPolicy<HashSet<string>>
    {
        public HashSet<string> Create() => new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Return(HashSet<string> obj)
        {
            obj.Clear();
            return true;
        }
    }
}
