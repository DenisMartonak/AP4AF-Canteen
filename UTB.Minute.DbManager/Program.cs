using Microsoft.AspNetCore.Http.HttpResults;
using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<MinuteDbContext>("database");

var app = builder.Build();

app.MapPost("/dev/seed", async Task<NoContent> (MinuteDbContext context) =>
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    var meal = new Meal { Name = "Smažák", Description = "Smažený sýr s hranolkami", Price = 120 };
    context.Meals.Add(meal);

    var menuItem = new MenuItem { Date = DateOnly.FromDateTime(DateTime.Now), Meal = meal, AvailablePortions = 10 };
    context.MenuItems.Add(menuItem);

    await context.SaveChangesAsync();
    return TypedResults.NoContent();
});

app.Run();
