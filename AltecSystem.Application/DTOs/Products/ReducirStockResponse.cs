using System;

namespace AltecSystem.Application.DTOs.Products
{
    public class ReducirStockResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
    }
}
