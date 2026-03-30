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
    public class AumentarStockCommandHandler : IRequestHandler<AumentarStockCommand, ReducirStockResponse>
    {
        private readonly AltecSystemDbContext _dbContext;

        public AumentarStockCommandHandler(AltecSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReducirStockResponse> Handle(AumentarStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Productos.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new InvalidOperationException("Producto no encontrado.");

            if (request.Quantity <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a 0.");

            product.Stock += request.Quantity;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ReducirStockResponse
            {
                ProductId = product.Id,
                Stock = product.Stock
            };
        }
    }
}
