using System.Threading.Tasks;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using System.Collections.Generic;
using System.Linq;
using AltecSystem.Application.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<SaleDto>> GetAllSalesAsync()
        {
            return await _context.Sales
                .Select(s => new SaleDto
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    EmployeeId = s.EmployeeId,
                    ProductId = s.ProductId,
                    SaleDate = s.SaleDate,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TaxAmount = s.TaxAmount,
                    TotalAmount = s.TotalAmount,
                    PaymentMethod = s.PaymentMethod,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    Profit = s.Profit
                })
                .ToListAsync();
        }

        public async Task<List<SalesBasketDto>> GetSalesGroupedByInvoiceNumberAsync()
        {
            return await _context.Sales
                .GroupBy(s => s.InvoiceNumber)
                .Select(g => new SalesBasketDto
                {
                    InvoiceNumber = g.Key,
                    Sales = g.Select(s => new SaleDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        ProductId = s.ProductId,
                        SaleDate = s.SaleDate,
                        Quantity = s.Quantity,
                        UnitPrice = s.UnitPrice,
                        TaxAmount = s.TaxAmount,
                        TotalAmount = s.TotalAmount,
                        PaymentMethod = s.PaymentMethod,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt,
                        Profit = s.Profit
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
