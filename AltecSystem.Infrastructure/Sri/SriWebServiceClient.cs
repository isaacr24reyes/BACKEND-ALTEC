using System.Globalization;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Xml.Linq;
using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Application.Interfaces;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// Cliente SOAP de los web services offline del SRI:
///   RecepcionComprobantesOffline   -> validarComprobante(xml en base64)  => RECIBIDA / DEVUELTA
///   AutorizacionComprobantesOffline -> autorizacionComprobante(clave)    => AUTORIZADO / NO AUTORIZADO
/// </summary>
public class SriWebServiceClient : ISriWebServiceClient
{
    private const string PruebasBase = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/";
    private const string ProduccionBase = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/";

    private readonly HttpClient _http;

    public SriWebServiceClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<SriReceptionResult> SendAsync(string signedXml, byte ambiente, CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml));
        var envelope =
            "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ec=\"http://ec.gob.sri.ws.recepcion\">" +
            "<soapenv:Header/><soapenv:Body><ec:validarComprobante>" +
            $"<xml>{base64}</xml>" +
            "</ec:validarComprobante></soapenv:Body></soapenv:Envelope>";

        var response = await PostAsync(BaseUrl(ambiente) + "RecepcionComprobantesOffline", envelope, ct);

        var respuesta = response.Descendants().FirstOrDefault(e => e.Name.LocalName == "RespuestaRecepcionComprobante");
        return new SriReceptionResult
        {
            Estado = Value(respuesta, "estado") ?? string.Empty,
            Mensajes = ParseMessages(respuesta)
        };
    }

    public async Task<SriAuthorizationResult> AuthorizeAsync(string claveAcceso, byte ambiente, CancellationToken ct = default)
    {
        var envelope =
            "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ec=\"http://ec.gob.sri.ws.autorizacion\">" +
            "<soapenv:Header/><soapenv:Body><ec:autorizacionComprobante>" +
            $"<claveAccesoComprobante>{SecurityElement.Escape(claveAcceso)}</claveAccesoComprobante>" +
            "</ec:autorizacionComprobante></soapenv:Body></soapenv:Envelope>";

        var response = await PostAsync(BaseUrl(ambiente) + "AutorizacionComprobantesOffline", envelope, ct);

        var autorizacion = response.Descendants().FirstOrDefault(e => e.Name.LocalName == "autorizacion");
        if (autorizacion == null)
            return new SriAuthorizationResult(); // el SRI aún no tiene respuesta para esa clave

        var estado = Value(autorizacion, "estado");
        var fecha = Value(autorizacion, "fechaAutorizacion");
        var comprobante = Value(autorizacion, "comprobante");

        return new SriAuthorizationResult
        {
            Estado = estado,
            NumeroAutorizacion = Value(autorizacion, "numeroAutorizacion"),
            FechaAutorizacion = DateTimeOffset.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f) ? f : null,
            Mensajes = ParseMessages(autorizacion),
            XmlAutorizacion = estado == SriInvoiceStates.Autorizado
                ? BuildAuthorizationXml(autorizacion, comprobante)
                : null
        };
    }

    private async Task<XDocument> PostAsync(string url, string envelope, CancellationToken ct)
    {
        using var content = new StringContent(envelope, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("SOAPAction", "\"\"");

        using var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"El SRI respondió {(int)response.StatusCode}: {Truncate(body, 300)}");

        return XDocument.Parse(body);
    }

    private static string BaseUrl(byte ambiente) => ambiente == 2 ? ProduccionBase : PruebasBase;

    private static string? Value(XElement? parent, string localName) =>
        parent?.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;

    /// <summary>Los mensajes vienen como &lt;mensaje&gt;&lt;identificador/&gt;&lt;mensaje/&gt;&lt;informacionAdicional/&gt;&lt;tipo/&gt;&lt;/mensaje&gt;.</summary>
    private static List<SriMessageDto> ParseMessages(XElement? parent)
    {
        if (parent == null) return new List<SriMessageDto>();

        return parent.Descendants()
            .Where(e => e.Name.LocalName == "mensaje" && e.Elements().Any(c => c.Name.LocalName == "identificador"))
            .Select(m => new SriMessageDto
            {
                Identificador = Value(m, "identificador"),
                Mensaje = Value(m, "mensaje"),
                InformacionAdicional = Value(m, "informacionAdicional"),
                Tipo = Value(m, "tipo")
            })
            .ToList();
    }

    /// <summary>XML de autorización estándar (el que se entrega al cliente), con el comprobante en CDATA.</summary>
    private static string BuildAuthorizationXml(XElement autorizacion, string? comprobante)
    {
        var doc = new XElement("autorizacion",
            new XElement("estado", Value(autorizacion, "estado")),
            new XElement("numeroAutorizacion", Value(autorizacion, "numeroAutorizacion")),
            new XElement("fechaAutorizacion", Value(autorizacion, "fechaAutorizacion")),
            new XElement("ambiente", Value(autorizacion, "ambiente")),
            new XElement("comprobante", new XCData(comprobante ?? string.Empty)),
            new XElement("mensajes"));

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + doc.ToString(SaveOptions.DisableFormatting);
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max] + "…";
}
