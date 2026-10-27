using AltecSystem.Application.DTOs.ElectronicInvoices;
using MediatR;

namespace AltecSystem.Application.Commands.ElectronicInvoices;

public record RetryElectronicInvoiceCommand(Guid Id) : IRequest<ElectronicInvoiceResultDto>;

/// <summary>Envía o reenvía la factura por correo; Email reemplaza al del cliente si se indica.</summary>
public record SendElectronicInvoiceEmailCommand(Guid Id, string? Email) : IRequest<ElectronicInvoiceResultDto>;
