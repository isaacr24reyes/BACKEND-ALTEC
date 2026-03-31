using AltecSystem.Application.Commands.Products;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.Products
{
    public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public DeactivateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);

            if (product == null)
                throw new KeyNotFoundException($"Producto con ID {request.Id} no encontrado.");

            product.IsActive = false;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            product.UpdatedBy = "ADMIN";

            await _productRepository.UpdateAsync(product);

            return true;
        }
    }
}
