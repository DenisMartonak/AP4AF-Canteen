using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using UTB.Minute.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<MinuteDbContext>("database");

var app = builder.Build();

app.MapGet("/api/meals", async Task<Ok<MealDto[]>> (MinuteDbContext db) =>
{
    var meals = await db.Meals.Select(m => new MealDto(m.Id, m.Name, m.Description, m.Price, m.IsActive)).ToArrayAsync();
    return TypedResults.Ok(meals);
});

app.MapPost("/api/meals", async Task<Created<MealDto>> (MealRequestDto dto, MinuteDbContext db) =>
{
    var meal = new Meal { Name = dto.Name, Description = dto.Description, Price = dto.Price };
    db.Meals.Add(meal);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/meals/{meal.Id}", new MealDto(meal.Id, meal.Name, meal.Description, meal.Price, meal.IsActive));
});

app.MapPut("/api/meals/{id:int}", async Task<Results<NoContent, NotFound>> (int id, MealPutRequestDto dto, MinuteDbContext db) =>
{
    if (await db.Meals.FindAsync(id) is Meal meal)
    {
        meal.Name = dto.Name;
        meal.Description = dto.Description;
        meal.Price = dto.Price;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
});

app.MapPatch("/api/meals/{id:int}", async Task<Results<NoContent, NotFound>> (int id, MealPatchActiveDto dto, MinuteDbContext db) =>
{
    if (await db.Meals.FindAsync(id) is Meal meal)
    {
        meal.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
});

app.MapGet("/api/menuitems", async Task<Ok<MenuItemDto[]>> (MinuteDbContext db) =>
{
    var items = await db.MenuItems
        .Include(m => m.Meal)
        .Select(m => new MenuItemDto(m.Id, m.Date, m.MealId, m.Meal!.Name, m.AvailablePortions))
        .ToArrayAsync();
    return TypedResults.Ok(items);
});

app.MapPost("/api/menuitems", async Task<Results<Created<MenuItemDto>, BadRequest<string>>> (MenuItemRequestDto dto, MinuteDbContext db) =>
{
    var meal = await db.Meals.FindAsync(dto.MealId);
    if (meal == null || !meal.IsActive) return TypedResults.BadRequest("Jídlo neexistuje nebo je neaktivní.");

    var menuItem = new MenuItem { Date = dto.Date, MealId = dto.MealId, AvailablePortions = dto.AvailablePortions };
    db.MenuItems.Add(menuItem);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/menuitems/{menuItem.Id}", new MenuItemDto(menuItem.Id, menuItem.Date, menuItem.MealId, meal.Name, menuItem.AvailablePortions));
});

app.MapPut("/api/menuitems/{id:int}", async Task<Results<NoContent, NotFound>> (int id, MenuItemPutRequestDto dto, MinuteDbContext db) =>
{
    if (await db.MenuItems.FindAsync(id) is MenuItem menuItem)
    {
        menuItem.Date = dto.Date;
        menuItem.AvailablePortions = dto.AvailablePortions;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
});

app.MapDelete("/api/menuitems/{id:int}", async Task<Results<NoContent, NotFound>> (int id, MinuteDbContext db) =>
{
    if (await db.MenuItems.FindAsync(id) is MenuItem menuItem)
    {
        db.MenuItems.Remove(menuItem);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NotFound();
});


app.MapGet("/api/orders", async Task<Ok<OrderDto[]>> (MinuteDbContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.MenuItem)
        .ThenInclude(m => m!.Meal)
        .Select(o => new OrderDto(o.Id, o.MenuItemId, o.MenuItem!.Meal!.Name, o.State.ToString()))
        .ToArrayAsync();
    return TypedResults.Ok(orders);
});

app.MapPost("/api/orders", async Task<Results<Created<OrderDto>, BadRequest<string>>> (OrderRequestDto dto, MinuteDbContext db) =>
{
    var menuItem = await db.MenuItems.Include(m => m.Meal).FirstOrDefaultAsync(m => m.Id == dto.MenuItemId);

    if (menuItem == null || menuItem.AvailablePortions <= 0)
        return TypedResults.BadRequest("Porce nejsou dostupné.");

    menuItem.AvailablePortions--;

    var order = new Order { MenuItemId = menuItem.Id, State = OrderState.Preparing };
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/orders/{order.Id}", new OrderDto(order.Id, order.MenuItemId, menuItem.Meal!.Name, order.State.ToString()));
});

app.MapPatch("/api/orders/{id:int}/state", async Task<Results<NoContent, NotFound, BadRequest<string>>> (int id, OrderStatePatchDto dto, MinuteDbContext db) =>
{
    if (await db.Orders.FindAsync(id) is Order order)
    {
        if (Enum.TryParse<OrderState>(dto.State, out var newState))
        {
            if (order.State == OrderState.Cancelled && newState != OrderState.Cancelled)
            {
                return TypedResults.BadRequest("Zrušenou objednávku nelze obnovit.");
            }

            order.State = newState;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        return TypedResults.BadRequest("Neplatný stav objednávky.");
    }
    return TypedResults.NotFound();
});

app.Run();