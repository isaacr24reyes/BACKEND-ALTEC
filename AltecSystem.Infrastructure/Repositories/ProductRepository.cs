using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Infrastructure.Persistence;
namespace AltecSystem.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AltecSystemDbContext _context;

    public ProductRepository(AltecSystemDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product)
    {
        await _context.Productos.AddAsync(product);
        await _context.SaveChangesAsync();
    }
}