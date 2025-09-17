using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Extensions;
using LAV.GraphDbFramework.Core.Mapping;
using LAV.GraphDbFramework.Core.Repositories;
using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Examples.Models;
using Microsoft.Extensions.ObjectPool;

namespace LAV.GraphDbFramework.Examples.Repositories;

public class UserRepository : BaseGraphRepository, IUserRepository
{
	private readonly ObjectPool<User>? _userPool;
	private readonly ObjectPool<Dictionary<string, object>>? _paramPool;

	public UserRepository(IGraphDbUnitOfWork unitOfWork) : base(unitOfWork)
	{
		_userPool = null;
		_paramPool = null;
	}
	public UserRepository(IServiceProvider provider) : base(provider)
		//base(provider.GetRequiredService<IGraphUnitOfWorkFactory>())
	{
		//var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
		//_userPool = poolProvider.Create<User>(new DefaultPooledObjectPolicy<User>());

		_userPool = provider.GetRequiredService<ObjectPool<User>>();
		_paramPool = provider.GetRequiredService<ObjectPool<Dictionary<string, object>>>();
			//poolProvider.Create<Dictionary<string, object>>(new ParameterPoolPolicy());
	}

	public async ValueTask<IReadOnlyList<User>> FindAsync(ISpecification specification)
	{
		var querySpec = new QuerySpecification<User>()
			.Match(specification)
			.Return("n");

		return await FindAsync(querySpec);
	}

	public async ValueTask<IReadOnlyList<User>> FindAsync(QuerySpecification<User> querySpecification)
	{
		var query = querySpecification.BuildQuery();
		var parameters = querySpecification.Parameters;

		return await UnitOfWork.RunAsync<User>(query, parameters);
	}

	public async ValueTask<User> FindOneAsync(ISpecification specification)
	{
		var results = await FindAsync(specification);
		return results.FirstOrDefault();
	}

	public async ValueTask<User> FindOneAsync(QuerySpecification<User> querySpecification)
	{
		var results = await FindAsync(querySpecification);
		return results.FirstOrDefault();
	}

	public async ValueTask<int> CountAsync(ISpecification specification)
	{
		var querySpec = new QuerySpecification<User>()
			.Match(specification)
			.Return("COUNT(n) AS count");

		var result = await UnitOfWork.RunAsync<CountResult>(querySpec.BuildQuery(), querySpec.Parameters);
		return result.FirstOrDefault()?.Count ?? 0;
	}

	public async ValueTask<bool> ExistsAsync(ISpecification specification)
	{
		return await CountAsync(specification) > 0;
	}

	private class CountResult
	{
		public int Count { get; set; }
	}

	public async ValueTask<User?> CreateAsync(User user)
	{
		user.Id ??= Guid.NewGuid().ToString();
		user.CreatedAt = DateTime.UtcNow;

		// Используем сгенерированный маппер для преобразования в свойства
		var properties = MapperCache<User>.MapToProperties(user);

		//using var pooledParams = new PooledDictionary(_paramPool);
		//var parameters = pooledParams.Dictionary.ToDictionary();
		//parameters["properties"] = properties;
		var parameters = new Dictionary<string, object>
		{
			["properties"] = properties
		};

		var results = await UnitOfWork.RunAsync<User>(
			"CREATE (u:User $properties) RETURN u{.*}",
			parameters);

		return results?.FirstOrDefault();
	}

	public async ValueTask<User?> UpdateAsync(User user)
	{
		// Используем сгенерированный маппер для преобразования в свойства
		var properties = MapperCache<User>.MapToProperties(user);

		var parameters = new Dictionary<string, object>
		{
			["id"] = user.Id!,
			["properties"] = properties
		};

		var uow = UnitOfWork;

		var results = await uow.RunAsync<User>(
			"MATCH (u:User {id: $id}) SET u += $properties RETURN u{.*}",
			parameters);

		return results?.FirstOrDefault();
	}

	public async ValueTask<User?> CreateWithPoolAsync(Action<User> initialize)
	{
		var user = _userPool.Get();
		try
		{
			user.Id = Guid.NewGuid().ToString();
			user.CreatedAt = DateTime.UtcNow;

			initialize?.Invoke(user);

			// Используем сгенерированный маппер для преобразования в свойства
			var properties = MapperCache<User>.MapToProperties(user);

			//using var pooledParams = new PooledDictionary(_paramPool);
			//var parameters = pooledParams.Dictionary;
			//parameters["id"] = user.Id;
			//parameters["properties"] = properties;

			var parameters = new Dictionary<string, object>
			{
				["id"] = user.Id,
				["properties"] = properties
			};

			var results = await UnitOfWork.RunAsync<User>(
				"CREATE (u:User $properties) RETURN u{.*}",
				parameters);

			return results.FirstOrDefault();
		}
		finally
		{
			_userPool.Return(user);
		}
	}

	public async ValueTask<User?> GetByIdAsync(string id)
	{
		var results = await UnitOfWork.RunAsync<User>(
			"MATCH (u:User {id: $id}) RETURN u",
			new { id });

		return results?.FirstOrDefault();
	}

	public async ValueTask DeleteAsync(string id)
	{
		await UnitOfWork.RunAsync<User>(
			"MATCH (u:User {id: $id}) DETACH DELETE u",
			new { id });
	}

	public async ValueTask<IReadOnlyList<User>> GetByAgeRangeAsync(int minAge, int maxAge)
	{
		return await UnitOfWork.RunAsync<User>(
			"MATCH (u:User) WHERE u.age >= $minAge AND u.age <= $maxAge RETURN u",
			new { minAge, maxAge });
	}
}