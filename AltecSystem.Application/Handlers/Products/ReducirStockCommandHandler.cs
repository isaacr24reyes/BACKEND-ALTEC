using System;
using System.Threading;
using System.Threading.Tasks;
using AltecSystem.Application.Commands.Products;
using AltecSystem.Application.DTOs.Products;
using AltecSystem.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Application.Handlers.Products
{
    public class ReducirStockCommandHandler : IRequestHandler<ReducirStockCommand, ReducirStockResponse>
    {
        private readonly AltecSystemDbContext _dbContext;

        public ReducirStockCommandHandler(AltecSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReducirStockResponse> Handle(ReducirStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Productos.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new InvalidOperationException("Producto no encontrado.");

            if (product.Stock < request.Quantity)
                throw new InvalidOperationException("Stock insuficiente para realizar la operación.");

            product.Stock -= request.Quantity;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ReducirStockResponse
            {
                ProductId = product.Id,
                Stock = product.Stock
            };
        }
    }
}
