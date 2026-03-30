using AltecSystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltecSystem.Application.Interfaces
{
    public interface IQuotationRepository
    {
        Task GuardarAsync(List<QuotationDetail> quotationDetails);
        Task<List<QuotationDetail>> ObtenerPorNumeroCotizacionAsync(string quotationNumber);
        Task<List<QuotationDetail>> ObtenerTodasAsync();
        Task<bool> ExisteAsync(string quotationNumber);
    }
}
