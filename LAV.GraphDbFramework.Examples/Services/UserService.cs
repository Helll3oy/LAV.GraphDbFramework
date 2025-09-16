using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Examples.Models;
using LAV.GraphDbFramework.Examples.Repositories;
using Microsoft.Extensions.ObjectPool;
using System;

namespace LAV.GraphDbFramework.Examples.Services;

public class UserService : IAsyncDisposable
{
	private readonly IUserRepository _userRepository;
	private readonly ObjectPool<SpecificationBuilder.PooledSpecificationBuilder<User>> _specBuilderPool;

	public UserService(IUserRepository userRepository)
	{
		_userRepository = userRepository;

		var poolProvider = new DefaultObjectPoolProvider();
		_specBuilderPool = poolProvider.Create<SpecificationBuilder.PooledSpecificationBuilder<User>>(
			new SpecificationBuilderPoolPolicy());
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