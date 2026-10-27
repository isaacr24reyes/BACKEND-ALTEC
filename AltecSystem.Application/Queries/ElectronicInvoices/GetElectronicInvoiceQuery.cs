using AltecSystem.Application.DTOs.ElectronicInvoices;
using MediatR;

namespace AltecSystem.Application.Queries.ElectronicInvoices;

public record GetElectronicInvoiceQuery(Guid Id) : IRequest<ElectronicInvoiceResultDto?>;

/// <summary>PDF (RIDE) de la factura: bytes y número de factura para el nombre del archivo.</summary>
public record GetElectronicInvoicePdfQuery(Guid Id) : IRequest<(byte[] Pdf, string NumeroFactura)?>;

/// <summary>Histórico paginado de facturas electrónicas.</summary>
public record ListElectronicInvoicesQuery(ElectronicInvoiceFilter Filter) : IRequest<ElectronicInvoicePageDto>;
