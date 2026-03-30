using System;

namespace AltecSystem.Domain.Entities
{
    public class QuotationDetail
    {
        public Guid Id { get; set; }
        public string QuotationNumber { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string PriceType { get; set; } // "pvp" o "mayorista"
        public DateTime CreatedAt { get; set; }
    }
}
