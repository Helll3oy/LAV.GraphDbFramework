using Microsoft.Extensions.ObjectPool;

namespace LAV.GraphDbFramework.Core.Pooling;

internal sealed class ParameterPoolPolicy : IPooledObjectPolicy<Dictionary<string, object>>
{
	public Dictionary<string, object> Create() => new Dictionary<string, object>(16);

	public bool Return(Dictionary<string, object> obj)
	{
		obj.Clear();
		return true;
	}
}
