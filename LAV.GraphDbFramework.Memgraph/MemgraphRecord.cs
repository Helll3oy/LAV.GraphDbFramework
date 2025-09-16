using LAV.GraphDbFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using LAV.GraphDbFramework.Core.Extensions;

namespace LAV.GraphDbFramework.Memgraph;

internal sealed class MemgraphRecord : Core.IRecord
{
	private readonly IRecord _record;
	private readonly Dictionary<string, object>? _properties;
	private readonly bool _hasProperties;

	public MemgraphRecord(IRecord record)
	{
		_record = record;
		if (_record.Values.Values.FirstOrDefault() is Dictionary<string, object> props)
		{
			_hasProperties = true;
			_properties = props;
		}
		else
		{
			_hasProperties = false;
			_properties = null;
		}
	}

	public T Get<T>(string key) => _record[key].As<T>();

	public bool TryGet<T>(string key, out T? value)
	{
		if (_hasProperties && _properties!.TryGetValue(key, out object? prop))
		{
			if (prop is ZonedDateTime zonedDateTime)
			{
				value = zonedDateTime.UtcDateTime.As<T>();
			}
			else if (prop is LocalDateTime localDateTime)
			{
				value = localDateTime.ToDateTime().As<T>();
			}
			else
			{
				value = prop.As<T>();
			}
				
			return true;
		}

		value = default;
		return false;
	}

	public Node GetNode(string key)
	{
		var node = _record[key].As<INode>();
		return new Node(
			node.ElementId,
			node.Properties.ToDictionary(p => p.Key, p => p.Value),
			node.Labels.ToList()
		);
	}

	public Relationship GetRelationship(string key)
	{
		var rel = _record[key].As<IRelationship>();
		return new Relationship(
			rel.ElementId,
			GetNode(key + "_start"),
			GetNode(key + "_end"),
			rel.Type,
			rel.Properties.ToDictionary(p => p.Key, p => p.Value)
		);
	}

	public IEnumerable<T> GetList<T>(string key)
	{
		return _record[key].As<IEnumerable<object>>().Select(item => (T)item);
	}

	public IReadOnlyDictionary<string, object> Properties =>
		_record.Keys.ToDictionary(k => k, k => (object)_record[k]);
}