using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class RelationshipSpecification : BaseSpecification
{
	private string _type;
	private Direction _direction = Direction.Outgoing;
	private readonly List<(string property, object value, ComparisonType comparison)> _properties = new();
	private string _alias = "r";
	private int? _minHops;
	private int? _maxHops;

	public RelationshipSpecification WithAlias(string alias)
	{
		_alias = alias;
		return this;
	}

	public RelationshipSpecification WithType(string type)
	{
		_type = type;
		return this;
	}

	public RelationshipSpecification WithDirection(Direction direction)
	{
		_direction = direction;
		return this;
	}

	public RelationshipSpecification WithHops(int? minHops, int? maxHops)
	{
		_minHops = minHops;
		_maxHops = maxHops;
		return this;
	}

	public RelationshipSpecification WithProperty(string property, object value, ComparisonType comparison = ComparisonType.Equals)
	{
		_properties.Add((property, value, comparison));
		return this;
	}

	public override string BuildQuery()
	{
		var sb = new StringBuilder();

		// Определяем направление
		switch (_direction)
		{
			case Direction.Outgoing:
				sb.Append("-");
				break;
			case Direction.Incoming:
				sb.Append("<-");
				break;
			case Direction.Bidirectional:
				sb.Append("-");
				break;
		}

		// Добавляем алиас и тип отношения
		sb.Append($"[{_alias}");

		if (!string.IsNullOrEmpty(_type))
		{
			sb.Append($":{_type}");
		}

		// Добавляем диапазон hops для переменной длины
		if (_minHops.HasValue || _maxHops.HasValue)
		{
			var min = _minHops.HasValue ? _minHops.Value.ToString() : "";
			var max = _maxHops.HasValue ? _maxHops.Value.ToString() : "";
			sb.Append($"*{min}..{max}");
		}

		// Добавляем свойства
		if (_properties.Any())
		{
			sb.Append(" {");

			var propertyConditions = new List<string>();
			foreach (var (property, value, comparison) in _properties)
			{
				var paramName = GenerateParameterName(property);
				AddParameter(paramName, value);

				string condition;
				switch (comparison)
				{
					case ComparisonType.Equals:
						condition = $"{property}: ${paramName}";
						break;
					case ComparisonType.NotEquals:
						condition = $"NOT {_alias}.{property} = ${paramName}";
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
						condition = $"{property}: ${paramName}";
						break;
				}

				propertyConditions.Add(condition);
			}

			// Для простых равенств используем синтаксис {property: value}
			var equalityConditions = _properties
				.Where(p => p.comparison == ComparisonType.Equals)
				.Select(p => $"{p.property}: ${GenerateParameterName(p.property)}");

			if (equalityConditions.Any())
			{
				sb.Append(string.Join(", ", equalityConditions));
			}

			sb.Append("}");

			// Для не-равенств добавляем WHERE условия
			if (_properties.Any(p => p.comparison != ComparisonType.Equals))
			{
				sb.Append($" WHERE {string.Join(" AND ", propertyConditions)}");
			}
		}
		else
		{
			sb.Append("]");
		}

		// Завершаем направление
		switch (_direction)
		{
			case Direction.Outgoing:
				sb.Append("->");
				break;
			case Direction.Incoming:
				sb.Append("-");
				break;
			case Direction.Bidirectional:
				sb.Append("-");
				break;
		}

		return sb.ToString();
	}
}