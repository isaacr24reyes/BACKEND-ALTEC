namespace AltecSystem.Application.DTOs.Sales
{
    public class CreateSaleDto
    {
        public required string InvoiceNumber { get; set; }
        public Guid? EmployeeId { get; set; } // Cambiado a GUID
        public Guid ProductId { get; set; } // Ya es GUID
        public DateTime? SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public required string PaymentMethod { get; set; }
        public string? Status { get; set; }
        public decimal? Profit { get; set; } // Agregado para incluir el campo Profit
    }
}
