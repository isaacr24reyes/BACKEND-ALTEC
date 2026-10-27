using AltecSystem.Application.DTOs.ElectronicInvoices;
using MediatR;

namespace AltecSystem.Application.Commands.ElectronicInvoices;

public record CreateElectronicInvoiceCommand(CreateElectronicInvoiceRequest Request) : IRequest<ElectronicInvoiceResultDto>;
