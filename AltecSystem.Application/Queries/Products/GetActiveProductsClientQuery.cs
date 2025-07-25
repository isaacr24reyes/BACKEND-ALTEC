using AltecSystem.Application.DTOs.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class GetActiveProductsClientQuery : IRequest<PaginatedResult<ProductClientDto>>
{
    public int PageNumber { get; set; }

    [BindRequired]
    public int PageSize { get; set; }

    public string? Filter { get; set; }
    public string? OrderBy { get; set; }
    public string? SortOrder { get; set; } 
}