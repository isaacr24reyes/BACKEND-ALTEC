using AltecSystem.Application.Interfaces;

namespace AltecSystem.Application.Handlers.Products;

using DTOs.Product;
using MediatR;
using System.Linq.Dynamic.Core;
using System.Linq;

public class GetActiveProductsClientHandler : IRequestHandler<GetActiveProductsClientQuery, PaginatedResult<ProductClientDto>>
{
    private readonly IProductRepository _productRepository;

    public GetActiveProductsClientHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PaginatedResult<ProductClientDto>> Handle(GetActiveProductsClientQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetActiveProductsAsync();
        var query = products.AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            var filter = request.Filter.ToLower();
            query = query.Where(p =>
                p.Descripcion.ToLower().Contains(filter) ||
                p.Codigo.ToLower().Contains(filter));
        }

        // Validar que OrderBy sea una propiedad válida
        var validProperties = new[] { "Id", "Descripcion", "Codigo", "Categoria", "Pvp", "Foto", "Stock" };
        if (!string.IsNullOrWhiteSpace(request.OrderBy) && !validProperties.Contains(request.OrderBy))
        {
            throw new ArgumentException($"El campo OrderBy '{request.OrderBy}' no es válido.");
        }

        // Ordenamiento dinámico
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            var isDescending = request.SortOrder?.ToLower() == "desc";
            var orderByExpression = request.OrderBy + (isDescending ? " descending" : "");
            query = query.OrderBy(orderByExpression);
        }

        // Total antes de paginar
        var total = query.Count();

        // Paginación y selección
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductClientDto
            {
                Id = p.Id,
                Descripcion = p.Descripcion,
                Codigo = p.Codigo,
                Categoria = p.Categoria,
                Pvp = p.Pvp,
                Foto = p.Foto,
                Stock = p.Stock
            })
            .ToList();

        return new PaginatedResult<ProductClientDto>(items, total);
    }

}