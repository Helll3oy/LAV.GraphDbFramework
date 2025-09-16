using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class NodeSpecification : BaseSpecification
{
	private readonly HashSet<string> _labels = [];
	private readonly HashSet<(string property, string paramName, ComparisonType comparison)> _properties = [];
	private string _alias = "n";

	public NodeSpecification WithAlias(string alias)
	{
		_alias = alias;
		return this;
	}

	public NodeSpecification WithLabel(string label)
	{
		_labels.Add(label);
		return this;
	}

	public NodeSpecification WithProperty(string property, string paramName, ComparisonType comparison = ComparisonType.Equals)
	{
		_properties.Add((property, paramName, comparison));
		return this;
	}

	public override string BuildQuery()
	{
		var sb = new StringBuilder();

		// Добавляем алиас
		sb.Append($"({_alias}");

		// Добавляем метки
		if (_labels.Count > 0)
		{
			sb.Append($":{string.Join(":", _labels)}");
		}

		// Добавляем свойства
		if (_properties.Count > 0)
		{
			sb.Append(" {");

			var equalityProps = _properties
				.Where(p => p.comparison == ComparisonType.Equals)
				.Select(p => $"{p.property}: ${p.paramName}");

			if (equalityProps.Any())
			{
				sb.Append(string.Join(", ", equalityProps));
			}

			sb.Append("}");
		}
		else
		{
			sb.Append(")");
		}

		// Добавляем условия WHERE для не-равенств
		var conditions = new List<string>();
		foreach (var (property, paramName, comparison) in _properties)
		{
			if (comparison != ComparisonType.Equals)
			{
				string condition;
				switch (comparison)
				{
					case ComparisonType.NotEquals:
						condition = $"{_alias}.{property} <> ${paramName}";
						break;
					case ComparisonType.GreaterThan:
						condition = $"{_alias}.{property} > ${paramName}";
						break;
					case ComparisonType.GreaterThanOrEqual:
						condition = $"{_alias}.{property} >= ${paramName}";
						break;
					case ComparisonType.LessThan:
						condition = $"{_alias}.{property} < ${paramName}";
						break;
					case ComparisonType.LessThanOrEqual:
						condition = $"{_alias}.{property} <= ${paramName}";
						break;
					case ComparisonType.Contains:
						condition = $"{_alias}.{property} CONTAINS ${paramName}";
						break;
					case ComparisonType.StartsWith:
						condition = $"{_alias}.{property} STARTS WITH ${paramName}";
						break;
					case ComparisonType.EndsWith:
						condition = $"{_alias}.{property} ENDS WITH ${paramName}";
						break;
					default:
						continue;
				}

				conditions.Add(condition);
			}
		}

		if (conditions.Count > 0)
		{
			sb.Append($" WHERE {string.Join(" AND ", conditions)}");
		}

		return sb.ToString();
	}
}