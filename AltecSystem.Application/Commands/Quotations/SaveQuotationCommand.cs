using AltecSystem.Application.DTOs.Quotations;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Commands.Quotations
{
    public class SaveQuotationCommand : IRequest<List<QuotationDetailResponse>>
    {
        public required string QuotationNumber { get; set; } // Modificado para requerir inicialización
        public required List<QuotationDetailRequest> QuotationDetails { get; set; } // Modificado para requerir inicialización
    }
}
