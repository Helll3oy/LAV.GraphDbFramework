//using LAV.GraphDbFramework.Core.Pooling;

namespace LAV.GraphDbFramework.Core.Repositories
{
	public interface IBaseGraphRepository
	{
		ValueTask SaveChangesAsync();
	}
}