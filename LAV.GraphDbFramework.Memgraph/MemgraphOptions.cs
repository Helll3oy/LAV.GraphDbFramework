using LAV.GraphDbFramework.Core;

namespace LAV.GraphDbFramework.Memgraph;

public class MemgraphOptions : BaseGraphDbOptions<MemgraphExtraOptions>;

public class MemgraphExtraOptions
{
    public string? TestOption { get; set; }
}