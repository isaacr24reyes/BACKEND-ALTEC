using System;
using System.Threading.Tasks;
using AltecSystem.Application.DTOs.Products;

namespace AltecSystem.Application.Services
{
    public interface IProductService
    {
        Task<ReducirStockResponse> ReducirStockAsync(Guid productId, int quantity);
    }
}
