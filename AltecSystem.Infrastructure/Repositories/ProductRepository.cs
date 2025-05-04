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
        // Devuelve los productos activos sin aplicar filtros o paginación
        var products = await _context.Productos
            .Where(p => p.IsActive == true)  // Filtro para productos activos
            .ToListAsync();  // Aquí se realiza la carga de los datos de la base de datos

        // Asegurarnos de que el campo Foto no sea NULL
        foreach (var product in products)
        {
            product.Foto = product.Foto ?? "NOT-IMAGE";
        }

        return products;
    }


}