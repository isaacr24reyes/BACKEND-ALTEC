using System;

namespace AltecSystem.Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid? EmployeeID { get; set; } // Cambiado a GUID
        public Guid ProductID { get; set; } // Ya es GUID
        public DateTime? SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? Profit { get; set; } // Agregado para incluir el campo Profit
    }
}
