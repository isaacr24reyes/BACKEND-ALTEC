using System.Xml;
using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

/// <summary>Configuración de facturación electrónica tomada de appsettings / variables de entorno.</summary>
public interface ISriConfiguration
{
    /// <summary>1 = pruebas, 2 = producción.</summary>
    byte Ambiente { get; }
}

/// <summary>Construye el XML de la factura (versión 1.1.0) sin firmar.</summary>
public interface ISriInvoiceXmlBuilder
{
    XmlDocument Build(ElectronicInvoice invoice, SriSettings settings);
}

/// <summary>Firma un comprobante con XAdES-BES usando el certificado .p12 configurado.</summary>
public interface ISriXmlSigner
{
    string Sign(XmlDocument comprobante);
}

/// <summary>Cliente de los web services offline del SRI (recepción y autorización).</summary>
public interface ISriWebServiceClient
{
    Task<SriReceptionResult> SendAsync(string signedXml, byte ambiente, CancellationToken ct = default);
    Task<SriAuthorizationResult> AuthorizeAsync(string claveAcceso, byte ambiente, CancellationToken ct = default);
}
