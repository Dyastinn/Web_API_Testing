using Microsoft.EntityFrameworkCore;
using SimpleApi.Application.Data;
using SimpleApi.Domain.Entities;

namespace SimpleApi.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }
        return product;
    }

    public async Task<Product> Create(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Product name cannot be empty", nameof(product.Name));
        }

        if (product.Price <= 0)
        {
            throw new ArgumentException("Product price must be greater than 0", nameof(product.Price));
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> Update(int id, Product product)
    {
        var existingProduct = await _context.Products.FindAsync(id);
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
        _context.Products.Update(existingProduct);
        await _context.SaveChangesAsync();
        return existingProduct;
    }

    public async Task Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
