using System.Globalization;
using System.Net;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AltecSystem.Infrastructure.Services;

/// <summary>
/// Envío de facturas por SMTP (Gmail por defecto). Configuración en la sección "Email":
///   Email:SmtpHost  (smtp.gmail.com)   Email:SmtpPort (587)
///   Email:User      cuenta que envía    Email:Password  contraseña de aplicación (NUNCA en el repositorio)
///   Email:FromName  nombre visible del remitente
/// </summary>
public class SmtpInvoiceEmailSender : IInvoiceEmailSender
{
    private readonly IConfiguration _config;

    public SmtpInvoiceEmailSender(IConfiguration config)
    {
        _config = config;
    }

    private string Host => _config["Email:SmtpHost"] is { Length: > 0 } h ? h : "smtp.gmail.com";
    private int Port => int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
    private string? User => _config["Email:User"];
    // Google muestra la contraseña de aplicación en grupos con espacios; se quitan
    private string? Password => _config["Email:Password"]?.Replace(" ", "");
    private string FromName => _config["Email:FromName"] is { Length: > 0 } n ? n : "ALTEC MECATRÓNICA";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(User) && !string.IsNullOrWhiteSpace(Password)
                                && !Password!.StartsWith("PON_AQUI", StringComparison.Ordinal);

    public async Task SendInvoiceAsync(ElectronicInvoice invoice, SriSettings settings, string? to, string? copy,
        byte[] pdf, string xml, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("El correo no está configurado (Email:User / Email:Password).");
        if (string.IsNullOrWhiteSpace(to) && string.IsNullOrWhiteSpace(copy))
            throw new InvalidOperationException("No hay destinatario para la factura.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(FromName, User));
        if (!string.IsNullOrWhiteSpace(to))
            message.To.Add(new MailboxAddress(invoice.RazonSocialComprador, to));
        if (!string.IsNullOrWhiteSpace(copy))
        {
            // Si no hay correo del cliente, la copia va como destinatario principal
            if (message.To.Count == 0) message.To.Add(MailboxAddress.Parse(copy));
            else message.Bcc.Add(MailboxAddress.Parse(copy));
        }

        var ambiente = invoice.Ambiente == 2 ? "" : " [PRUEBAS]";
        message.Subject = $"Factura electrónica {invoice.NumeroFactura} - {settings.NombreComercial ?? settings.RazonSocial}{ambiente}";

        var total = invoice.ImporteTotal.ToString("0.00", CultureInfo.InvariantCulture);
        var fecha = invoice.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var body = new BodyBuilder
        {
            HtmlBody =
                "<div style=\"font-family:Arial,sans-serif;font-size:14px;color:#222\">" +
                $"<p>Estimado(a) <b>{WebUtility.HtmlEncode(invoice.RazonSocialComprador)}</b>,</p>" +
                $"<p>Adjuntamos su <b>factura electrónica N° {invoice.NumeroFactura}</b> del {fecha} por un total de <b>${total}</b>, " +
                "autorizada por el SRI.</p>" +
                $"<p style=\"font-size:12px;color:#555\">Clave de acceso / N° de autorización:<br>{invoice.NumeroAutorizacion ?? invoice.ClaveAcceso}</p>" +
                "<p>Adjuntos: la factura en PDF (RIDE) y el archivo XML, que es el comprobante electrónico válido ante el SRI.</p>" +
                $"<p>Gracias por su compra.<br><b>{WebUtility.HtmlEncode(settings.NombreComercial ?? settings.RazonSocial)}</b><br>" +
                $"{WebUtility.HtmlEncode(settings.DirEstablecimiento)}</p></div>"
        };
        body.Attachments.Add($"Factura-{invoice.NumeroFactura}.pdf", pdf, new ContentType("application", "pdf"));
        body.Attachments.Add($"{invoice.ClaveAcceso}.xml", System.Text.Encoding.UTF8.GetBytes(xml), new ContentType("application", "xml"));
        message.Body = body.ToMessageBody();

        // El certificado del servidor se valida igual; solo se omite la consulta de revocación (CRL/OCSP),
        // que .NET no logra completar en macOS.
        using var smtp = new SmtpClient { CheckCertificateRevocation = false };
        await smtp.ConnectAsync(Host, Port, SecureSocketOptions.StartTls, ct);
        await smtp.AuthenticateAsync(User, Password, ct);
        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
