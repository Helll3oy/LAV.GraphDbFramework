using LAV.GraphDbFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace LAV.GraphDbFramework.Neo4j;

internal sealed class Neo4jRecord : Core.IRecord
{
    private readonly IRecord _record;

    public Neo4jRecord(IRecord record)
    {
        _record = record;
    }

    public T Get<T>(string key) => _record[key].As<T>();

    public bool TryGet<T>(string key, out T? value)
    {
        if (!_record.Keys.Contains(key))
        {
            value = default;
            return false;
        }

        try
        {
            value = _record[key].As<T>();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
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