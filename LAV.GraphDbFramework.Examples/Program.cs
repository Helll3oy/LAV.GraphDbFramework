using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Examples.Models;
using LAV.GraphDbFramework.Examples.Repositories;
using LAV.GraphDbFramework.Examples.Services;
using Microsoft.Extensions.ObjectPool;
using LAV.GraphDbFramework.Client;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddGraphDb(builder.Configuration);
builder.Services.AddMemgraphGraphDbClient(builder.Configuration);
//builder.Services.AddNeo4jGraphDbClient(builder.Configuration);

builder.Services.AddSingleton(provider =>
{
	var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
	return poolProvider.Create<User>(new DefaultPooledObjectPolicy<User>());
});

//builder.Services.AddScoped<IUserRepository, UserRepository>(provider =>
//{
//	//var pool = provider.GetRequiredService<ObjectPool<IGraphUnitOfWork>>();
//	return new UserRepository(provider);
//});
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<UserService>();

builder.Services.AddTransient<QuerySpecification<User>>();

var app = builder.Build();

app.MapGet("/", async (IUserRepository userRepository) =>
{ 
	return await userRepository.GetByIdAsync("admin");
});

app.MapPost("/users", async (IUserRepository userRepository) =>
{
	await userRepository.CreateAsync(new User
	{
		Id = "admin",
		Email = "admin@komifoms.ru",
		Name = "Admin",
		CreatedAt = DateTime.Now,
	});

	var result = await userRepository.UpdateAsync(new User
	{
		Id = "admin",
		Email = "admin@komifoms.ru",
		Name = "ChangedAdmin",
		Age = 18,
	});

	await userRepository.SaveChangesAsync();

	return result;
});

await app.RunAsync();
