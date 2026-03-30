using MediatR;

namespace AltecSystem.Application.Commands.Sales
{
    public class CancelInvoiceCommand : IRequest<Unit>
    {
        public string InvoiceNumber { get; set; }

        public CancelInvoiceCommand(string invoiceNumber)
        {
            InvoiceNumber = invoiceNumber;
        }
    }
}
