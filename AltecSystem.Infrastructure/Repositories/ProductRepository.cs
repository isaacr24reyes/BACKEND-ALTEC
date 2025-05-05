using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        var products = await _context.Productos
            .Where(p => p.IsActive == true) 
            .ToListAsync();

        // Asegurarnos de que el campo Foto no sea NULL
        foreach (var product in products)
        {
            product.Foto = product.Foto ?? "NOT-IMAGE";
        }

        return products;
    }
    public async Task UpdateAsync(Product product)
    {
        _context.Productos.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Productos.FindAsync(id);
    }

}