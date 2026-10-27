using System.Globalization;
using System.Xml;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// Genera el XML de la factura electrónica versión 1.1.0 (esquema offline del SRI).
/// El orden de los elementos sigue el XSD oficial; no se usa sangría para que la firma
/// se calcule sobre el mismo contenido que se envía.
/// </summary>
public class SriInvoiceXmlBuilder : ISriInvoiceXmlBuilder
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public XmlDocument Build(ElectronicInvoice invoice, SriSettings settings)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

        var factura = doc.CreateElement("factura");
        factura.SetAttribute("id", "comprobante");
        factura.SetAttribute("version", "1.1.0");
        doc.AppendChild(factura);

        // ── infoTributaria ──
        var trib = Add(factura, "infoTributaria");
        Add(trib, "ambiente", invoice.Ambiente.ToString(Inv));
        Add(trib, "tipoEmision", "1");
        Add(trib, "razonSocial", Text(settings.RazonSocial, 300));
        AddIf(trib, "nombreComercial", Text(settings.NombreComercial, 300));
        Add(trib, "ruc", settings.Ruc);
        Add(trib, "claveAcceso", invoice.ClaveAcceso);
        Add(trib, "codDoc", "01");
        Add(trib, "estab", invoice.Estab);
        Add(trib, "ptoEmi", invoice.PtoEmi);
        Add(trib, "secuencial", invoice.Secuencial.ToString("D9", Inv));
        Add(trib, "dirMatriz", Text(settings.DirMatriz, 300));
        AddIf(trib, "agenteRetencion", settings.AgenteRetencion);
        AddIf(trib, "contribuyenteRimpe", Text(settings.ContribuyenteRimpe, 300));

        // ── infoFactura ──
        var info = Add(factura, "infoFactura");
        Add(info, "fechaEmision", invoice.FechaEmision.ToString("dd/MM/yyyy", Inv));
        AddIf(info, "dirEstablecimiento", Text(settings.DirEstablecimiento, 300));
        AddIf(info, "contribuyenteEspecial", settings.ContribuyenteEspecial);
        Add(info, "obligadoContabilidad", settings.ObligadoContabilidad);
        Add(info, "tipoIdentificacionComprador", invoice.TipoIdentificacionComprador);
        Add(info, "razonSocialComprador", Text(invoice.RazonSocialComprador, 300));
        Add(info, "identificacionComprador", invoice.IdentificacionComprador);
        AddIf(info, "direccionComprador", Text(invoice.DireccionComprador, 300));
        Add(info, "totalSinImpuestos", Money(invoice.TotalSinImpuestos));
        Add(info, "totalDescuento", Money(invoice.TotalDescuento));

        var totalConImpuestos = Add(info, "totalConImpuestos");
        foreach (var grupo in invoice.Items.GroupBy(i => i.CodigoPorcentajeIva))
        {
            var ti = Add(totalConImpuestos, "totalImpuesto");
            Add(ti, "codigo", "2"); // IVA
            Add(ti, "codigoPorcentaje", grupo.Key);
            Add(ti, "baseImponible", Money(grupo.Sum(i => i.PrecioTotalSinImpuesto)));
            Add(ti, "valor", Money(grupo.Sum(i => i.ValorIva)));
        }

        Add(info, "propina", "0.00");
        Add(info, "importeTotal", Money(invoice.ImporteTotal));
        Add(info, "moneda", "DOLAR");

        var pagos = Add(info, "pagos");
        foreach (var p in invoice.Payments)
        {
            var pago = Add(pagos, "pago");
            Add(pago, "formaPago", p.FormaPago);
            Add(pago, "total", Money(p.Total));
            Add(pago, "plazo", p.Plazo.ToString(Inv));
            Add(pago, "unidadTiempo", p.UnidadTiempo);
        }

        // ── detalles ──
        var detalles = Add(factura, "detalles");
        foreach (var item in invoice.Items.OrderBy(i => i.Linea))
        {
            var det = Add(detalles, "detalle");
            Add(det, "codigoPrincipal", Text(item.CodigoPrincipal, 25));
            Add(det, "descripcion", Text(item.Descripcion, 300));
            Add(det, "cantidad", Qty(item.Cantidad));
            Add(det, "precioUnitario", Qty(item.PrecioUnitario));
            Add(det, "descuento", Money(item.Descuento));
            Add(det, "precioTotalSinImpuesto", Money(item.PrecioTotalSinImpuesto));

            var impuestos = Add(det, "impuestos");
            var imp = Add(impuestos, "impuesto");
            Add(imp, "codigo", "2");
            Add(imp, "codigoPorcentaje", item.CodigoPorcentajeIva);
            Add(imp, "tarifa", item.TarifaIva.ToString("0.0#", Inv));
            Add(imp, "baseImponible", Money(item.PrecioTotalSinImpuesto));
            Add(imp, "valor", Money(item.ValorIva));
        }

        // ── infoAdicional (solo si hay datos) ──
        var adicionales = new List<(string Nombre, string Valor)>();
        if (!string.IsNullOrWhiteSpace(invoice.EmailComprador))
            adicionales.Add(("Email", Text(invoice.EmailComprador, 300)!));
        if (!string.IsNullOrWhiteSpace(invoice.TelefonoComprador))
            adicionales.Add(("Telefono", Text(invoice.TelefonoComprador, 300)!));

        if (adicionales.Count > 0)
        {
            var infoAd = Add(factura, "infoAdicional");
            foreach (var (nombre, valor) in adicionales)
            {
                var campo = Add(infoAd, "campoAdicional", valor);
                campo.SetAttribute("nombre", nombre);
            }
        }

        return doc;
    }

    private static XmlElement Add(XmlElement parent, string name, string? value = null)
    {
        var el = parent.OwnerDocument.CreateElement(name);
        if (value != null) el.InnerText = value;
        parent.AppendChild(el);
        return el;
    }

    private static void AddIf(XmlElement parent, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) Add(parent, name, value);
    }

    private static string Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.00", Inv);

    private static string Qty(decimal value) => Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.00####", Inv);

    /// <summary>Limpia saltos de línea y espacios repetidos, y recorta a la longitud máxima del XSD.</summary>
    private static string? Text(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var clean = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return clean.Length > max ? clean[..max] : clean;
    }
}
