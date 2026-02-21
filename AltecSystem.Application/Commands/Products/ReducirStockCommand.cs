using AltecSystem.Application.DTOs.Products;
using MediatR;
using System;

namespace AltecSystem.Application.Commands.Products
{
    public class ReducirStockCommand : IRequest<ReducirStockResponse>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public ReducirStockCommand(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
