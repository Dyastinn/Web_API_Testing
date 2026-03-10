using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleApi.Domain.Entities;

namespace SimpleApi.IntegrationTests;

public class ProductsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetProducts_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        var products = await response.Content.ReadAsAsync<List<Product>?>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(products ?? new());
    }

    [Fact]
    public async Task PostProduct_CreatesProduct()
    {
        // Arrange
        var product = new { name = "Laptop", price = 1000 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", product);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdProduct = await response.Content.ReadAsAsync<Product>();
        Assert.NotNull(createdProduct);
        Assert.NotEqual(0, createdProduct.Id);
        Assert.Equal("Laptop", createdProduct.Name);
        Assert.Equal(1000, createdProduct.Price);
    }

    [Fact]
    public async Task PostProduct_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var product = new { name = "Product", price = -100 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", product);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        // Arrange
        var createProduct = new { name = "Mouse", price = 50 };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var createdProduct = await createResponse.Content.ReadAsAsync<Product>();
        Assert.NotNull(createdProduct);

        // Act
        var response = await _client.GetAsync($"/api/products/{createdProduct.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadAsAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal(createdProduct.Id, product.Id);
    }

    [Fact]
    public async Task GetProductById_InvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutProduct_UpdatesProduct()
    {
        // Arrange
        var createProduct = new { name = "Keyboard", price = 75 };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var createdProduct = await createResponse.Content.ReadAsAsync<Product>();
        Assert.NotNull(createdProduct);
        var updateProduct = new { name = "Mechanical Keyboard", price = 150 };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{createdProduct.Id}", updateProduct);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedProduct = await response.Content.ReadAsAsync<Product>();
        Assert.NotNull(updatedProduct);
        Assert.Equal("Mechanical Keyboard", updatedProduct.Name);
        Assert.Equal(150, updatedProduct.Price);
    }

    [Fact]
    public async Task PutProduct_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var product = new { name = "Monitor", price = 300 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/products/999", product);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_RemovesProduct()
    {
        // Arrange
        var createProduct = new { name = "Headphones", price = 200 };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var createdProduct = await createResponse.Content.ReadAsAsync<Product>();
        Assert.NotNull(createdProduct);

        // Act
        var response = await _client.DeleteAsync($"/api/products/{createdProduct.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_InvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/products/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsErrorResponse()
    {
        // Act - attempt to parse invalid id
        var response = await _client.GetAsync("/api/products/notanumber");

        // Assert - route constraint validation returns 404 or BadRequest
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
    }
}

internal class ErrorResponse
{
    public string? Message { get; set; }
    public int StatusCode { get; set; }
}

internal static class HttpContentExtensions
{
    internal static async Task<T?> ReadAsAsync<T>(this HttpContent content)
    {
        var jsonContent = await content.ReadAsStringAsync();
        return string.IsNullOrEmpty(jsonContent) 
            ? default 
            : JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
