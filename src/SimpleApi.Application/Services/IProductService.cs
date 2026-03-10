using SimpleApi.Domain.Entities;

namespace SimpleApi.Application.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAll();
    Task<Product> GetById(int id);
    Task<Product> Create(Product product);
    Task<Product> Update(int id, Product product);
    Task Delete(int id);
}
