using LAV.GraphDbFramework.Core.Repositories;
using LAV.GraphDbFramework.Examples.Models;

namespace LAV.GraphDbFramework.Examples.Repositories;

public interface IUserRepository : IBaseGraphRepository, ISpecificationRepository<User>
{
	ValueTask<User?> GetByIdAsync(string id);
	ValueTask<User?> CreateAsync(User user);
	ValueTask<User?> UpdateAsync(User user);
	ValueTask DeleteAsync(string id);
	ValueTask<IReadOnlyList<User>> GetByAgeRangeAsync(int minAge, int maxAge);
}
