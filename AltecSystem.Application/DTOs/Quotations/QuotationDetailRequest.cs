namespace AltecSystem.Application.DTOs.Quotations
{
    public class QuotationDetailRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Agregado para incluir el precio unitario
    }
}
