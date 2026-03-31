using MediatR;

namespace AltecSystem.Application.Commands.Products
{
    public class DeactivateProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
