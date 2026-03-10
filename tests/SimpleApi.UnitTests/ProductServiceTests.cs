using Xunit;
using SimpleApi.Application.Services;
using SimpleApi.Domain.Entities;
using SimpleApi.UnitTests.Fixtures;

namespace SimpleApi.UnitTests;

public class ProductServiceTests : IAsyncLifetime
{
    private readonly AppDbContextFixture _fixture = new();
    private ProductService _service = null!;

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _service = new ProductService(_fixture.Context);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task GetAll_ReturnsProducts()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };
        await _service.Create(product);

        // Act
        var result = await _service.GetAll();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };
        var created = await _service.Create(product);

        // Act
        var result = await _service.GetById(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Product 1", result.Name);
    }

    [Fact]
    public async Task GetById_InvalidId_ThrowsException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetById(999));
    }

    [Fact]
    public async Task Create_AddsProduct()
    {
        // Arrange
        var product = new Product { Name = "Laptop", Price = 1000 };

        // Act
        var result = await _service.Create(product);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(1000, result.Price);
    }

    [Fact]
    public async Task Create_InvalidName_ThrowsException()
    {
        // Arrange
        var product = new Product { Name = "", Price = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Create(product));
    }

    [Fact]
    public async Task Create_InvalidPrice_ThrowsException()
    {
        // Arrange
        var product = new Product { Name = "Product", Price = -100 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Create(product));
    }

    [Fact]
    public async Task Update_ChangesProduct()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };
        var created = await _service.Create(product);
        var updated = new Product { Name = "Updated Product", Price = 200 };

        // Act
        var result = await _service.Update(created.Id, updated);

        // Assert
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal(200, result.Price);
    }

    [Fact]
    public async Task Update_InvalidId_ThrowsException()
    {
        // Arrange
        var product = new Product { Name = "Product", Price = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Update(999, product));
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        // Arrange
        var product = new Product { Name = "Product 1", Price = 100 };
        var created = await _service.Create(product);

        // Act
        await _service.Delete(created.Id);

        // Assert
        var all = await _service.GetAll();
        Assert.Empty(all);
    }

    [Fact]
    public async Task Delete_InvalidId_ThrowsException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Delete(999));
    }
}
