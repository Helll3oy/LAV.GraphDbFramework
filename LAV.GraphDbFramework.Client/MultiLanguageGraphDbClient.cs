using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using LAV.GraphDbFramework.QueryLanguage;

namespace LAV.GraphDbFramework.Client;

public class MultiLanguageGraphDbClient : IGraphDbClient
{
	private readonly IGraphDbClient _innerClient;
	private readonly QueryLanguageProviderFactory _providerFactory;
	private readonly ILogger<MultiLanguageGraphDbClient> _logger;

	public MultiLanguageGraphDbClient(IGraphDbClient innerClient, QueryLanguageProviderFactory providerFactory,
		ILogger<MultiLanguageGraphDbClient> logger = null)
	{
		_innerClient = innerClient;
		_providerFactory = providerFactory;
		_logger = logger;
	}

	public IGraphDbUnitOfWorkFactory UnitOfWorkFactory => _innerClient.UnitOfWorkFactory;

	public async ValueTask<T> ExecuteReadAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		return await _innerClient.ExecuteReadAsync(operation);
	}

	public async ValueTask<T> ExecuteWriteAsync<T>(Func<IGraphDbQueryRunner, ValueTask<T>> operation)
	{
		return await _innerClient.ExecuteWriteAsync(operation);
	}

	public async ValueTask<IGraphDbUnitOfWork> BeginUnitOfWorkAsync()
	{
		return await _innerClient.BeginUnitOfWorkAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await _innerClient.DisposeAsync();
	}

	public async Task<IReadOnlyList<T>> ExecuteQueryAsync<T>(QueryLanguageType language, string query, object parameters = null)
	{
		var provider = _providerFactory.CreateProvider(language, _innerClient);
		return await provider.ExecuteQueryAsync<T>(query, parameters);
	}

	public async Task<IReadOnlyList<T>> ExecuteQueryAsync<T>(QueryLanguageType language, string query,
		object parameters, Func<IGraphDbRecord, T> mapper)
	{
		var provider = _providerFactory.CreateProvider(language, _innerClient);
		return await provider.ExecuteQueryAsync(query, parameters, mapper);
	}

	public string FormatQuery(QueryLanguageType language, string query, object parameters)
	{
		var provider = _providerFactory.CreateProvider(language, _innerClient);
		return provider.FormatQuery(query, parameters);
	}
}
