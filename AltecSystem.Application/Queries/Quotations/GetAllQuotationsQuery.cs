using AltecSystem.Application.DTOs.Quotations;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Queries.Quotations
{
    public class GetAllQuotationsQuery : IRequest<List<QuotationSummaryDto>>
    {
    }
}
