using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Memgraph;
using LAV.GraphDbFramework.Neo4j;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Client;

public static class GraphDbFactory
{
    public static IGraphClient CreateClient(GraphDbOptions options, ILoggerFactory loggerFactory)
    {
        return options.DbType switch
        {
            GraphDbType.Neo4j => new Neo4jClient(options.Uri!, options.Username, options.Password, loggerFactory),
            GraphDbType.Memgraph => new MemgraphClient(options.Host!, options.Username, options.Password, loggerFactory),
            _ => throw new NotSupportedException($"Unsupported database: {options.DbType}")
        };
    }
}
