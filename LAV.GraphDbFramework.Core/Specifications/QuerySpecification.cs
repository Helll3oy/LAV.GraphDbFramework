using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class QuerySpecification<T> : BaseSpecification
{
	private readonly List<ISpecification> _matchClauses = new();
	private readonly List<string> _whereClauses = new();
	private readonly List<string> _returnClauses = new();
	private readonly List<string> _orderByClauses = new();
	private int? _limit;
	private int? _skip;

	public QuerySpecification<T> Match(ISpecification specification)
	{
		_matchClauses.Add(specification);
		return this;
	}

	public QuerySpecification<T> Where(ISpecification specification)
	{
		_whereClauses.Add(specification.BuildQuery());

		// Добавляем параметры
		foreach (var param in specification.Parameters)
		{
			AddParameter(param.Key, param.Value);
		}

		return this;
	}

	public QuerySpecification<T> Return(string returnClause)
	{
		_returnClauses.Add(returnClause);
		return this;
	}

	public QuerySpecification<T> OrderBy(string property, bool descending = false)
	{
		_orderByClauses.Add(descending ? $"{property} DESC" : property);
		return this;
	}

	public QuerySpecification<T> Limit(int limit)
	{
		_limit = limit;
		return this;
	}

	public QuerySpecification<T> Skip(int skip)
	{
		_skip = skip;
		return this;
	}

	public override string BuildQuery()
	{
		var sb = new StringBuilder();

		// MATCH clauses
		if (_matchClauses.Any())
		{
			sb.Append("MATCH ");
			sb.Append(string.Join(", ", _matchClauses.Select(m => m.BuildQuery())));
			sb.AppendLine();
		}

		// WHERE clauses
		if (_whereClauses.Any())
		{
			sb.Append("WHERE ");
			sb.Append(string.Join(" AND ", _whereClauses));
			sb.AppendLine();
		}

		// RETURN clause
		if (_returnClauses.Any())
		{
			sb.Append("RETURN ");
			sb.Append(string.Join(", ", _returnClauses));
			sb.AppendLine();
		}
		else
		{
			// По умолчанию возвращаем все узлы и отношения из MATCH
			sb.AppendLine("RETURN *");
		}

		// ORDER BY clause
		if (_orderByClauses.Any())
		{
			sb.Append("ORDER BY ");
			sb.Append(string.Join(", ", _orderByClauses));
			sb.AppendLine();
		}

		// SKIP clause
		if (_skip.HasValue)
		{
			sb.AppendLine($"SKIP {_skip.Value}");
		}

		// LIMIT clause
		if (_limit.HasValue)
		{
			sb.AppendLine($"LIMIT {_limit.Value}");
		}

		return sb.ToString();
	}
}