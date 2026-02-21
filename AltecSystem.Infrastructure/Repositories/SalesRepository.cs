using System.Threading.Tasks;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Infrastructure.Persistence; // Asegúrate de tener la referencia correcta

namespace AltecSystem.Infrastructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly AltecSystemDbContext _context;

        public SalesRepository(AltecSystemDbContext context)
        {
            _context = context;
        }

        public async Task<Sale> AddSaleAsync(Sale sale)
        {
            // Eliminado el cálculo de Profit, ahora solo se guarda el valor proporcionado
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }
    }
}
