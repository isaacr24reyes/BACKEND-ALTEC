using AltecSystem.Application.DTOs.Quotations;
using MediatR;

namespace AltecSystem.Application.Commands.Quotations
{
    public class UpdateQuotationCommand : IRequest<List<QuotationDetailResponse>>
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public List<QuotationDetailRequest> QuotationDetails { get; set; } = new();
    }
}
