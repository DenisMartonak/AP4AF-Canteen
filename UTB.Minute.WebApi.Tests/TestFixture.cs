using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;

namespace UTB.Minute.WebApi.Tests;

public class TestFixture : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private string? _connectionString;
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.UTB_Minute_AppHost>(["--environment=Testing"]);

        _app = await builder.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications.WaitForResourceHealthyAsync("database");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("webapi");

        _connectionString = await _app.GetConnectionStringAsync("database");
        HttpClient = _app.CreateHttpClient("webapi", "http");

        using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var meal = new Meal { Name = "Testovací polévka", Description = "Vývar", Price = 40 };
        context.Meals.Add(meal);
        await context.SaveChangesAsync();
    }

    public MinuteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MinuteDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new MinuteDbContext(options);
    }

    public async Task DisposeAsync()
    {
        HttpClient.Dispose();
        await _app.DisposeAsync();
    }
}
