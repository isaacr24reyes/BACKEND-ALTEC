using AltecSystem.Application.DTOs.ElectronicInvoices;
using MediatR;

namespace AltecSystem.Application.Commands.ElectronicInvoices;

public record RetryElectronicInvoiceCommand(Guid Id) : IRequest<ElectronicInvoiceResultDto>;

/// <summary>Envía o reenvía la factura por correo; Email reemplaza al del cliente si se indica.</summary>
public record SendElectronicInvoiceEmailCommand(Guid Id, string? Email) : IRequest<ElectronicInvoiceResultDto>;

/// <summary>Registra la anulación hecha en SRI en línea y, si se pide, cancela la venta y devuelve el stock.</summary>
public record AnularElectronicInvoiceCommand(Guid Id, AnularElectronicInvoiceRequest Request) : IRequest<ElectronicInvoiceResultDto>;
