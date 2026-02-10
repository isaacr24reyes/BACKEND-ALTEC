namespace AltecSystem.Application.Interfaces
{
    using AltecSystem.Domain.Entities;
    using System.Threading.Tasks;

    public interface ISalesRepository
    {
        Task<Sale> AddSaleAsync(Sale sale);
    }
}
