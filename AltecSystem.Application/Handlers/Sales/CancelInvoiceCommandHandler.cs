using AltecSystem.Application.Commands.Sales;
using AltecSystem.Application.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Sales
{
    public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Unit>
    {
        private readonly ISalesRepository _salesRepository;

        public CancelInvoiceCommandHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<Unit> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                throw new InvalidOperationException("El número de factura no puede estar vacío.");

            await _salesRepository.CancelInvoiceAsync(request.InvoiceNumber);
            return Unit.Value;
        }
    }
}
