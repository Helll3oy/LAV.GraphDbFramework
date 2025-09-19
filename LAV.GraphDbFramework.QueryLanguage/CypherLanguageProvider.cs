using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.QueryLanguage;

public class CypherLanguageProvider : IQueryLanguageProvider
{
	private readonly IGraphDbClient _graphClient;
	private readonly ILogger<CypherLanguageProvider> _logger;

	public QueryLanguageType Language => QueryLanguageType.Cypher;
	public string LanguageName => "Cypher";

	public CypherLanguageProvider(IGraphDbClient graphClient, ILogger<CypherLanguageProvider>? logger = null)
	{
		_graphClient = graphClient;
		_logger = logger!;
	}

	public async ValueTask<IReadOnlyList<T>> ExecuteQueryAsync<T>(string query, object? parameters = null)
	{
		return await _graphClient.ExecuteReadAsync(async runner =>
			await runner.RunAsync<T>(query, parameters));
	}

	public async ValueTask<IReadOnlyList<T>> ExecuteQueryAsync<T>(string query, object? parameters, Func<IGraphDbRecord, T> mapper)
	{
		return await _graphClient.ExecuteReadAsync(async runner =>
			await runner.RunAsync(query, parameters, mapper));
	}

	public string FormatQuery(string query, object parameters)
	{
		if (parameters == null)
			return query;

		var formattedQuery = new StringBuilder(query);
		var props = parameters.GetType().GetProperties();

		foreach (var prop in props)
		{
			var value = prop.GetValue(parameters);
			var formattedValue = FormatValue(value);
			formattedQuery.Replace($"${prop.Name}", formattedValue);
		}

		return formattedQuery.ToString();
	}

	public (string Query, IReadOnlyDictionary<string, object> Parameters) ParseQuery(string query, object parameters)
	{
		// Для Cypher параметры уже в правильном формате
		return (query, parameters?.GetType().GetProperties()
			.ToDictionary(p => p.Name, p => p.GetValue(parameters)));
	}

	private string FormatValue(object? value)
	{
		if (value == null) return "null";
		if (value is string str) return $"\"{str.Replace("\"", "\\\"")}\"";
		if (value is DateTime dt) return $"datetime(\"{dt:yyyy-MM-ddTHH:mm:ssZ}\")";
		if (value is bool b) return b.ToString().ToLower();
		return value.ToString() ?? string.Empty;
	}
}