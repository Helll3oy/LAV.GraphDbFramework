using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Models;

public readonly record struct Relationship(
    string Id,
    Node StartNode,
    Node EndNode,
    string Type,
    IReadOnlyDictionary<string, object> Properties
);
