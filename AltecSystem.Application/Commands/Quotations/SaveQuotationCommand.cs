using AltecSystem.Application.DTOs.Quotations;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Commands.Quotations
{
    public class SaveQuotationCommand : IRequest<List<QuotationDetailResponse>>
    {
        public required List<QuotationDetailRequest> QuotationDetails { get; set; }
    }
}
