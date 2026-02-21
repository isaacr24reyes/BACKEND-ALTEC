using AltecSystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltecSystem.Application.Interfaces
{
    public interface IQuotationRepository
    {
        Task GuardarAsync(List<QuotationDetail> quotationDetails);
    }
}
