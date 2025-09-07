using AltecSystem.Application.DTOs.Product;
using MediatR;

namespace AltecSystem.Application.Queries.Products;

public record GetImportedProductsQuery : IRequest<ProductImportResponse>;
