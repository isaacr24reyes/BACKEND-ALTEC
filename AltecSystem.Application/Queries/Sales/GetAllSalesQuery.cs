using AltecSystem.Application.DTOs.Sales;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Queries.Sales
{
    public class GetAllSalesQuery : IRequest<List<SaleDto>>
    {
    }
}
