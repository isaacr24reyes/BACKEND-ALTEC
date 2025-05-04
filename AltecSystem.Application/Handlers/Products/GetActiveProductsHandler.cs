using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

public class GetActiveProductsHandler : IRequestHandler<GetActiveProductsQuery, PaginatedResult<Product>>
{
    private readonly IProductRepository _productRepository;

    public GetActiveProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PaginatedResult<Product>> Handle(GetActiveProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetActiveProductsAsync();
        var query = products.AsQueryable();
    
        // Filtros
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            var filter = request.Filter.ToLower(); // Convertimos el filtro a minúsculas
            query = query.Where(p =>
                p.Descripcion.ToLower().Contains(filter) ||   // Convertimos la columna a minúsculas
                p.Codigo.ToLower().Contains(filter));          // Convertimos la columna a minúsculas
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

        // Paginación
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();  // 'ToList' convierte a una lista en memoria, después de aplicar paginación

        return new PaginatedResult<Product>(items, total);
    }

}