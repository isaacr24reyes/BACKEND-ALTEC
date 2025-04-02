using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
}