using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.QueryLanguage;

public class QueryLanguageProviderFactory
{
	private readonly Dictionary<QueryLanguageType, Func<IGraphDbClient, IQueryLanguageProvider>> _providerFactories;
	private readonly ILogger<QueryLanguageProviderFactory> _logger;

	public QueryLanguageProviderFactory(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<QueryLanguageProviderFactory>();

		_providerFactories = new Dictionary<QueryLanguageType, Func<IGraphDbClient, IQueryLanguageProvider>>
		{
			[QueryLanguageType.Cypher] = (client) => new CypherLanguageProvider(client,
				loggerFactory.CreateLogger<CypherLanguageProvider>()),
			[QueryLanguageType.Gremlin] = (client) => new GremlinLanguageProvider(client,
				loggerFactory.CreateLogger<GremlinLanguageProvider>()),
		};
	}

	public IQueryLanguageProvider CreateProvider(QueryLanguageType language, IGraphDbClient graphClient)
	{
		if (_providerFactories.TryGetValue(language, out var factory))
		{
			return factory(graphClient);
		}

		throw new NotSupportedException($"Query language {language} is not supported");
	}

	public void RegisterProvider(QueryLanguageType language, Func<IGraphDbClient, IQueryLanguageProvider> factory)
	{
		_providerFactories[language] = factory;
		_logger?.LogInformation("Registered query language provider for {Language}", language);
	}

	public bool IsLanguageSupported(QueryLanguageType language)
	{
		return _providerFactories.ContainsKey(language);
	}

	public IEnumerable<QueryLanguageType> GetSupportedLanguages()
	{
		return _providerFactories.Keys;
	}
}
