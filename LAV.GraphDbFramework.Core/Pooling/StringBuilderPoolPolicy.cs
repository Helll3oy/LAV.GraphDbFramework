using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace LAV.GraphDbFramework.Core.Pooling;

internal sealed class StringBuilderPoolPolicy : IPooledObjectPolicy<StringBuilder>
{
	public StringBuilder Create() => new StringBuilder(256);

	public bool Return(StringBuilder obj)
	{
		obj.Clear();
		return true;
	}
}
