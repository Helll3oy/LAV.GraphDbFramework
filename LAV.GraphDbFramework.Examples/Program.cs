using LAV.GraphDbFramework.Client;
using LAV.GraphDbFramework.Core.Configuration;
using LAV.GraphDbFramework.Core.Exceptions;
using LAV.GraphDbFramework.Core.Specifications;
using LAV.GraphDbFramework.Core.UnitOfWork;
using LAV.GraphDbFramework.Examples.Models;
using LAV.GraphDbFramework.Examples.Repositories;
using LAV.GraphDbFramework.Examples.Services;
using Microsoft.Extensions.ObjectPool;

var builder = WebApplication.CreateBuilder(args);

// Добавляем обработку ошибок
builder.Services.AddGraphDbErrorHandling(builder.Configuration);

//builder.Services.AddGraphDb(builder.Configuration);
builder.Services.AddMemgraphGraphDbClient(builder.Configuration);
//builder.Services.AddNeo4jGraphDbClient(builder.Configuration);

builder.Services.AddGraphDbLinqSupport();

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

//app.UseExceptionHandler();

//// Настройка pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    // Используем наш обработчик исключений
//    //app.UseExceptionHandler();

    app.UseGraphDbErrorHandling();
//}

app.MapGet("/users/{id}", async (string id, IUserRepository userRepository) =>
{
	try
	{
		var user = await userRepository.GetByIdAsync(id);

		return Results.Ok(user);
	}
	catch (Exception ex)
	{
		//throw;
        throw new GraphDbException($"User with id {id} not found", "NOT_FOUND_ERROR");
    }
});

app.MapGet("/users/tests", async (UserService userService) =>
{
	try
	{
		var users = await userService.GetActiveUsersAsync();

		return Results.Ok(users);
	}
	catch (Exception ex)
	{
		//throw;
		throw new GraphDbException(ex.Message, "TEST_ERROR", innerException: ex);
	}
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
