using SimpleApi.Domain.Entities;

namespace SimpleApi.Application.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public Task<IEnumerable<Product>> GetAll()
    {
        return Task.FromResult(_products.AsEnumerable());
    }

    public Task<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }
        return Task.FromResult(product);
    }

    public Task<Product> Create(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Product name cannot be empty", nameof(product.Name));
        }

        if (product.Price <= 0)
        {
            throw new ArgumentException("Product price must be greater than 0", nameof(product.Price));
        }

        product.Id = _nextId++;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product> Update(int id, Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Product name cannot be empty", nameof(product.Name));
        }

        if (product.Price <= 0)
        {
            throw new ArgumentException("Product price must be greater than 0", nameof(product.Price));
        }

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        return Task.FromResult(existingProduct);
    }

    public Task Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        _products.Remove(product);
        return Task.CompletedTask;
    }
}
