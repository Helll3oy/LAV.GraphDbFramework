using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Models;

public readonly record struct Node(
    string Id,
    IReadOnlyDictionary<string, object> Properties,
    IReadOnlyList<string> Labels
);
