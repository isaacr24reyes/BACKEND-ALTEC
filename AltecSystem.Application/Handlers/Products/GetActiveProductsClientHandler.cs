using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;
using System.Linq.Dynamic.Core;
using AltecSystem.Application.DTOs.Product;
using Microsoft.EntityFrameworkCore;

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

        // Ordenamiento dinámico
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            var isDescending = request.SortOrder?.ToLower() == "desc";
            var orderByExpression = request.OrderBy + (isDescending ? " descending" : " ascending");
            query = query.OrderBy(orderByExpression);
        }

        // Total antes de paginar
        var total = query.Count();

        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductClientDto
            {
                Id = p.Id,
                Descripcion = p.Descripcion,
                Codigo = p.Codigo,
                Categoria = p.Categoria, // Si existe en la entidad
                Pvp = p.Pvp,
                Foto = p.Foto ?? "NOT-IMAGE"
            })

            .ToList();

        return new PaginatedResult<ProductClientDto>(items, total);
    }

}