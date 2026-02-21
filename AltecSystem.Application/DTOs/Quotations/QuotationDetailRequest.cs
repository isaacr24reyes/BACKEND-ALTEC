using System;

namespace AltecSystem.Application.DTOs.Quotations
{
    public class QuotationDetailRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
