using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
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
        try
        {
            var products = await _context.Productos
                .Where(p => p.IsActive == true)
                .ToListAsync();

            foreach (var product in products)
            {
                product.Foto = product.Foto ?? "NOT-IMAGE";
            }

            return products;
        }
        catch (SqlException ex)
        {
            Console.WriteLine("❌ SQL ERROR: " + ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GENERAL ERROR: " + ex.Message);
            throw;
        }
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