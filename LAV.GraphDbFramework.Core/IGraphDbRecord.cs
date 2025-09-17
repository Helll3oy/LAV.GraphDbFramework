using LAV.GraphDbFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public interface IGraphDbRecord
{
    T Get<T>(string key);
    bool TryGet<T>(string key, out T? value);
    IReadOnlyDictionary<string, object> Properties { get; }

    // Новые методы для работы с узлами и отношениями
    Node GetNode(string key);
    Relationship GetRelationship(string key);
    IEnumerable<T> GetList<T>(string key);
}
