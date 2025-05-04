using System.ComponentModel.DataAnnotations;
using AltecSystem.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class GetActiveProductsQuery : IRequest<PaginatedResult<Product>>
{
    [BindRequired]
    public int PageNumber { get; set; }

    [BindRequired]
    public int PageSize { get; set; }

    public string? Filter { get; set; }
    public string? OrderBy { get; set; }
    public string? SortOrder { get; set; } 
}