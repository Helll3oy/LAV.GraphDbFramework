using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Memgraph;
//using LAV.GraphDbFramework.Neo4j;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Client;

public class GraphDbClientFactory : IGraphDbClientFactory
{
	private readonly IServiceProvider _serviceProvider;

	//public static IGraphDbClient CreateClient(GraphDbOptions options, ILoggerFactory loggerFactory)
 //   {
 //       return options.DbType switch
 //       {
 //           GraphDbType.Neo4j => new Neo4jClient(options.Uri!, options.Username, options.Password, loggerFactory),
 //           GraphDbType.Memgraph => new MemgraphClient(options.Host!, options.Username, options.Password, loggerFactory),
 //           _ => throw new NotSupportedException($"Unsupported database: {options.DbType}")
 //       };
 //   }

	public GraphDbClientFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IGraphDbClient Create()
	{
		var options = _serviceProvider.GetRequiredService<IOptions<GraphDbOptions>>().Value;
		var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();


		return _serviceProvider.GetRequiredService<IGraphDbClient>();

        //return options.DbType switch
        //{
        //	GraphDbType.Neo4j => new Neo4jClient(options.Uri!, options.Username, options.Password, loggerFactory),
        //	GraphDbType.Memgraph => new MemgraphClient(options.Host!, options.Username, options.Password, loggerFactory),
        //	_ => throw new NotSupportedException($"Unsupported database: {options.DbType}")
        //};
    }
}
