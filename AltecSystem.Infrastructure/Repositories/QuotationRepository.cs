using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
    }
}
