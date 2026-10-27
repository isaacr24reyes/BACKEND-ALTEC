using System.Globalization;
using System.Numerics;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using AltecSystem.Application.Interfaces;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// Firma XAdES-BES "enveloped" para comprobantes del SRI, con la misma estructura que usa el
/// facturador oficial: RSA-SHA256, digests SHA-256, C14N 1.0, referencia a "#comprobante" y a las
/// SignedProperties (SigningTime, SigningCertificate, DataObjectFormat).
///
/// Los digests se calculan con el elemento ya ubicado en su posición final y con los namespaces
/// heredados de sus ancestros, que es lo que hará el SRI al verificar.
/// </summary>
public class SriXadesSigner : ISriXmlSigner
{
    private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";
    private const string Xades141Ns = "http://uri.etsi.org/01903/v1.4.1#";
    private const string Sha256 = "http://www.w3.org/2001/04/xmlenc#sha256";

    private readonly SriConfiguration _config;

    public SriXadesSigner(SriConfiguration config)
    {
        _config = config;
    }

    public string Sign(XmlDocument comprobante)
    {
        var cert = _config.GetCertificate();
        using var rsa = cert.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("La firma electrónica no tiene una clave RSA utilizable.");

        // Se trabaja sobre el XML parseado (namespaces explícitos, sin cambios de espacios)
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(comprobante.OuterXml);
        var root = doc.DocumentElement ?? throw new InvalidOperationException("Comprobante vacío.");

        // 1) Digest del comprobante (equivale a aplicar la transformación enveloped: aún no hay firma)
        var digestComprobante = Digest(root);

        // 2) Estructura de la firma
        var id = "xmldsig-" + Guid.NewGuid();
        var certB64 = Convert.ToBase64String(cert.RawData);
        var certDigest = Convert.ToBase64String(SHA256.HashData(cert.RawData));
        var issuer = SecurityElement.Escape(cert.IssuerName.Name.Replace(", ", ","));
        var serial = new BigInteger(cert.GetSerialNumber(), isUnsigned: true, isBigEndian: false)
            .ToString(CultureInfo.InvariantCulture);
        var signingTime = EcuadorNow().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture);

        var signatureXml =
            $"<ds:Signature xmlns:ds=\"{DsNs}\" Id=\"{id}\">" +
              "<ds:SignedInfo>" +
                "<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/>" +
                "<ds:SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\"/>" +
                $"<ds:Reference Id=\"{id}-ref0\" URI=\"#comprobante\">" +
                  "<ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></ds:Transforms>" +
                  $"<ds:DigestMethod Algorithm=\"{Sha256}\"/>" +
                  $"<ds:DigestValue>{digestComprobante}</ds:DigestValue>" +
                "</ds:Reference>" +
                $"<ds:Reference Type=\"http://uri.etsi.org/01903#SignedProperties\" URI=\"#{id}-signedprops\">" +
                  $"<ds:DigestMethod Algorithm=\"{Sha256}\"/>" +
                  "<ds:DigestValue></ds:DigestValue>" +
                "</ds:Reference>" +
              "</ds:SignedInfo>" +
              $"<ds:SignatureValue Id=\"{id}-sigvalue\"></ds:SignatureValue>" +
              $"<ds:KeyInfo><ds:X509Data><ds:X509Certificate>{certB64}</ds:X509Certificate></ds:X509Data></ds:KeyInfo>" +
              "<ds:Object>" +
                $"<xades:QualifyingProperties xmlns:xades=\"{XadesNs}\" xmlns:xades141=\"{Xades141Ns}\" Target=\"#{id}\">" +
                  $"<xades:SignedProperties Id=\"{id}-signedprops\">" +
                    "<xades:SignedSignatureProperties>" +
                      $"<xades:SigningTime>{signingTime}</xades:SigningTime>" +
                      "<xades:SigningCertificate><xades:Cert>" +
                        $"<xades:CertDigest><ds:DigestMethod Algorithm=\"{Sha256}\"/><ds:DigestValue>{certDigest}</ds:DigestValue></xades:CertDigest>" +
                        $"<xades:IssuerSerial><ds:X509IssuerName>{issuer}</ds:X509IssuerName><ds:X509SerialNumber>{serial}</ds:X509SerialNumber></xades:IssuerSerial>" +
                      "</xades:Cert></xades:SigningCertificate>" +
                    "</xades:SignedSignatureProperties>" +
                    "<xades:SignedDataObjectProperties>" +
                      $"<xades:DataObjectFormat ObjectReference=\"#{id}-ref0\">" +
                        "<xades:Description>FIRMA DIGITAL SRI</xades:Description>" +
                        "<xades:MimeType>text/xml</xades:MimeType>" +
                        "<xades:Encoding>UTF-8</xades:Encoding>" +
                      "</xades:DataObjectFormat>" +
                    "</xades:SignedDataObjectProperties>" +
                  "</xades:SignedProperties>" +
                "</xades:QualifyingProperties>" +
              "</ds:Object>" +
            "</ds:Signature>";

        var sigDoc = new XmlDocument { PreserveWhitespace = true };
        sigDoc.LoadXml(signatureXml);
        var signature = (XmlElement)doc.ImportNode(sigDoc.DocumentElement!, true);
        root.AppendChild(signature);

        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("ds", DsNs);
        ns.AddNamespace("xades", XadesNs);

        // 3) Digest de las SignedProperties, ya en su posición final
        var signedProps = (XmlElement)signature.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:SignedProperties", ns)!;
        signature.SelectSingleNode("ds:SignedInfo/ds:Reference[2]/ds:DigestValue", ns)!.InnerText = Digest(signedProps);

        // 4) Firma RSA-SHA256 de la SignedInfo canonicalizada
        var signedInfo = (XmlElement)signature.SelectSingleNode("ds:SignedInfo", ns)!;
        var signatureValue = rsa.SignData(Canonicalize(signedInfo), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        signature.SelectSingleNode("ds:SignatureValue", ns)!.InnerText = Convert.ToBase64String(signatureValue);

        return doc.OuterXml;
    }

    private static string Digest(XmlElement element) => Convert.ToBase64String(SHA256.HashData(Canonicalize(element)));

    /// <summary>C14N 1.0 inclusiva del subárbol, con los namespaces declarados en los ancestros (como al verificar).</summary>
    internal static byte[] Canonicalize(XmlElement element)
    {
        var standalone = new XmlDocument { PreserveWhitespace = true };
        var copy = (XmlElement)standalone.ImportNode(element, true);
        standalone.AppendChild(copy);

        for (var parent = element.ParentNode as XmlElement; parent != null; parent = parent.ParentNode as XmlElement)
        {
            foreach (XmlAttribute attr in parent.Attributes)
            {
                var isNsDecl = attr.Prefix == "xmlns" || attr.Name == "xmlns";
                if (isNsDecl && !copy.HasAttribute(attr.Name))
                    copy.SetAttribute(attr.Name, attr.Value);
            }
        }

        var transform = new XmlDsigC14NTransform();
        transform.LoadInput(standalone);
        using var output = (Stream)transform.GetOutput(typeof(Stream));
        using var ms = new MemoryStream();
        output.CopyTo(ms);
        return ms.ToArray();
    }

    private static DateTimeOffset EcuadorNow()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
    }
}
