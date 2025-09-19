using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core;

namespace LAV.GraphDbFramework.QueryLanguage;

public interface IQueryLanguageProvider
{
	QueryLanguageType Language { get; }
	string LanguageName { get; }

	ValueTask<IReadOnlyList<T>> ExecuteQueryAsync<T>(string query, object parameters = null);
	ValueTask<IReadOnlyList<T>> ExecuteQueryAsync<T>(string query, object parameters, Func<IGraphDbRecord, T> mapper);

	string FormatQuery(string query, object parameters);
	(string Query, IReadOnlyDictionary<string, object> Parameters) ParseQuery(string query, object parameters);
}
