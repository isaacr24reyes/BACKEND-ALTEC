using System;

namespace AltecSystem.Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public required string InvoiceNumber { get; set; } // Ya no es único, pero requerido
        public Guid? EmployeeId { get; set; } // Cambiado a GUID con nombre consistente
        public Guid ProductId { get; set; } // Cambiado a GUID con nombre consistente
        public DateTime? SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public required string PaymentMethod { get; set; } // Requerido para evitar errores de inicialización
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? Profit { get; set; } // Agregado para incluir el campo Profit
    }
}
