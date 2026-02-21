using System.Collections.Generic;

namespace AltecSystem.Application.DTOs.Sales
{
    public class SalesBasketDto
    {
        public string InvoiceNumber { get; set; }
        public List<SaleDto> Sales { get; set; }
    }
}
