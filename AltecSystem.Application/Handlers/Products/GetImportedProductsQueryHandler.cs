using AltecSystem.Application.DTOs.Product;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Products;
using MediatR;

public class GetImportedProductsQueryHandler : IRequestHandler<GetImportedProductsQuery, ProductImportResponse>
{
    private readonly IProductRepository _repository;

    public GetImportedProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductImportResponse> Handle(GetImportedProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetImportedProductsAsync();

        var items = products.Select(p => new ProductImportResponse.ProductItem
        {
            Id = p.Id,
            Categoria = p.Categoria,
            Codigo = p.Codigo,
            Stock = p.Stock,
            PrecioMayorista = p.PrecioMayorista,
            Descripcion = p.Descripcion,
            Foto = p.Foto,
            IsActive = p.IsActive,
            IsImport = p.IsImport
        }).ToList();

        return new ProductImportResponse
        {
            TotalCount = items.Count,
            Items = items
        };
    }
}