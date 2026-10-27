using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

/// <summary>Genera el RIDE (representación impresa) de la factura en PDF.</summary>
public interface IInvoicePdfGenerator
{
    byte[] Generate(ElectronicInvoice invoice, SriSettings settings);
}

/// <summary>Envía la factura (PDF + XML autorizado) por correo.</summary>
public interface IInvoiceEmailSender
{
    /// <summary>true si el correo está configurado (servidor, usuario y contraseña).</summary>
    bool IsConfigured { get; }

    /// <param name="to">Correo del cliente (puede ser null: entonces solo va la copia).</param>
    /// <param name="copy">Copia oculta (ej. el correo del negocio).</param>
    Task SendInvoiceAsync(ElectronicInvoice invoice, SriSettings settings, string? to, string? copy,
        byte[] pdf, string xml, CancellationToken ct = default);
}
