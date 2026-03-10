using Microsoft.EntityFrameworkCore;
using SimpleApi.Application.Data;
using Xunit;

namespace SimpleApi.UnitTests.Fixtures;

public class AppDbContextFixture : IAsyncLifetime
{
    private readonly AppDbContext _context;

    public AppDbContextFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
    }

    public AppDbContext Context => _context;

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
