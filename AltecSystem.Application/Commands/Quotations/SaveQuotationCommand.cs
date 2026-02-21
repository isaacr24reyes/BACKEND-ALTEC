using AltecSystem.Application.DTOs.Quotations;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Commands.Quotations
{
    public class SaveQuotationCommand : IRequest<List<QuotationDetailResponse>>
    {
        public string QuotationNumber { get; set; }
        public List<QuotationDetailRequest> QuotationDetails { get; set; }
    }
}
