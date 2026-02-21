using System;

namespace AltecSystem.Application.DTOs.Sales
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime? SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? Profit { get; set; }
    }
}
