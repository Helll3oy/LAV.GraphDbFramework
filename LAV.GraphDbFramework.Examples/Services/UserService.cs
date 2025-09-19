using LAV.GraphDbFramework.Core.Exceptions;
using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Examples.Models;
using LAV.GraphDbFramework.Examples.Repositories;
using LAV.GraphDbFramework.Linq;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Linq;

namespace LAV.GraphDbFramework.Examples.Services;

public class UserService : IAsyncDisposable
{
	private readonly IUserRepository _userRepository;
	private readonly IGraphDbQueryProvider _queryProvider;
	private readonly ObjectPool<SpecificationBuilder.PooledSpecificationBuilder<User>> _specBuilderPool;

	public UserService(IUserRepository userRepository, IGraphDbQueryProvider queryProvider)
	{
		_userRepository = userRepository;
		_queryProvider = queryProvider;

		var poolProvider = new DefaultObjectPoolProvider();
		_specBuilderPool = poolProvider.Create<SpecificationBuilder.PooledSpecificationBuilder<User>>(
			new SpecificationBuilderPoolPolicy());
	}

	public async ValueTask<User> GetUserByEmailAsync(string email)
	{
		try
		{
			var query = _queryProvider.CreateAsyncQuery<User>()
				.Where(u => u.Email == email && u.IsActive == true)
				.OrderBy(u => u.Name);

			return await query.FirstOrDefaultAsync();
		}
		catch (Exception ex)
		{
			throw new GraphDbQueryException($"Failed to get user by email: {email}", new { }, ex);
		}
	}

	public async ValueTask<List<User>> GetActiveUsersAsync()
	{
		try
		{
			var query = _queryProvider.CreateAsyncQuery<User>();
				//.Select(s => new User { 
				//	Age = s.Age, 
				//	Id = s.Id, 
				//	CreatedAt = s.CreatedAt, 
				//	Name = s.Name,
				//	Email = s.Email,
				//	IsActive = s.IsActive
				//});
				//.Where(u => u.IsActive == true)
				//.OrderBy(u => u.Name);

			return await query.ToListAsync();
		}
		catch (Exception ex)
		{
			throw new GraphDbQueryException("Failed to get active users", new { }, ex);
		}
	}

	public async Task<int> CountActiveUsersAsync()
	{
		try
		{
			var query = _queryProvider.CreateAsyncQuery<User>()
				.Where(u => u.IsActive == true);

			return await query.CountAsync();
		}
		catch (Exception ex)
		{
			throw new GraphDbQueryException("Failed to count active users", new { }, ex);
		}
	}

	public async ValueTask<IReadOnlyList<User>> GetActiveUsersByAgeRangeAsync(int minAge, int maxAge)
	{
		using var specBuilder = _specBuilderPool.Get();

		var (query, parameters) = specBuilder
			.WithLabel("User")
			.WithProperty("isActive", true)
			.WithProperty("age", minAge, ComparisonType.GreaterThanOrEqual)
			.WithProperty("age", maxAge, ComparisonType.LessThanOrEqual)
			.Build();

		throw new NotImplementedException();
		//return await _userRepository.FindAsync(new CustomSpecification(query, parameters));
	}

	public async ValueTask<IReadOnlyList<User>> SearchUsersAsync(string searchTerm, int? minAge = null)
	{
		using var specBuilder = _specBuilderPool.Get();

		specBuilder
			.WithLabel("User")
			.WithProperty("name", searchTerm, ComparisonType.Contains);

		if (minAge.HasValue)
		{
			specBuilder.WithProperty("age", minAge.Value, ComparisonType.GreaterThanOrEqual);
		}

		var (query, parameters) = specBuilder.Build();

		//throw new NotImplementedException();
		return await _userRepository.FindAsync(new CustomSpecification(query, parameters));
	}

	public async ValueTask<User?> CreateUserAsync(string name, string email, int age)
	{
		return await _userRepository.CreateAsync(new User
		{
			Name = name,
			Email = email,
			Age = age
		});
	}

	public ValueTask DisposeAsync()
	{
		return default;
	}

	private class SpecificationBuilderPoolPolicy : IPooledObjectPolicy<SpecificationBuilder.PooledSpecificationBuilder<User>>
	{
		public SpecificationBuilder.PooledSpecificationBuilder<User> Create()
		{
			return SpecificationBuilder.Create<User>();
		}

		public bool Return(SpecificationBuilder.PooledSpecificationBuilder<User> obj)
		{
			obj.Dispose();
			return true;
		}
	}
}