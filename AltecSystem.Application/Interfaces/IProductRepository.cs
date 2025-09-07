using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task UpdateAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetImportedProductsAsync();

}