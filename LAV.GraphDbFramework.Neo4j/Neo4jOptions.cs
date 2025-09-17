using LAV.GraphDbFramework.Core;

namespace LAV.GraphDbFramework.Neo4j;

public sealed class Neo4jOptions : BaseGraphDbOptions<Neo4jExtraOptions>;

public sealed class Neo4jExtraOptions
{
    public string? TestOption { get; set; }
}