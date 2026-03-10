using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleApi.Application.Data;

namespace SimpleApi.IntegrationTests.Fixtures;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the default DbContext registration if any
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database context with a fixed name for this factory instance
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Environment is set here
        });
    }
}

