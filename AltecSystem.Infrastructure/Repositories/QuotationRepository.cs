using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AltecSystem.Infrastructure.Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly AltecSystemDbContext _context;

        public QuotationRepository(AltecSystemDbContext context)
        {
            _context = context;
        }

        public async Task GuardarAsync(List<QuotationDetail> quotationDetails)
        {
            await _context.QuotationDetails.AddRangeAsync(quotationDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<List<QuotationDetail>> ObtenerPorNumeroCotizacionAsync(string quotationNumber)
        {
            return await _context.QuotationDetails
                .Where(q => q.QuotationNumber == quotationNumber)
                .ToListAsync();
        }

        public async Task<List<QuotationDetail>> ObtenerTodasAsync()
        {
            return await _context.QuotationDetails
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(string quotationNumber)
        {
            return await _context.QuotationDetails.AnyAsync(q => q.QuotationNumber == quotationNumber);
        }
    }
}
