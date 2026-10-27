using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using AltecSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// Configuración de facturación electrónica (sección "Sri" de appsettings o variables de entorno Sri__*):
///   Sri:Ambiente             1 = pruebas (por defecto), 2 = producción
///   Sri:CertificatePath      ruta al archivo .p12 de la firma electrónica (binario o en base64)
///   Sri:CertificatePassword  contraseña del .p12 (NUNCA en el repositorio)
/// </summary>
public class SriConfiguration : ISriConfiguration
{
    private readonly IConfiguration _config;
    private readonly object _lock = new();
    private X509Certificate2? _certificate;

    public SriConfiguration(IConfiguration config)
    {
        _config = config;
    }

    public byte Ambiente => byte.TryParse(_config["Sri:Ambiente"], out var a) && a is 1 or 2 ? a : (byte)1;

    /// <summary>Certificado de firma (con clave privada), cargado una sola vez.</summary>
    public X509Certificate2 GetCertificate()
    {
        if (_certificate != null) return _certificate;

        lock (_lock)
        {
            if (_certificate != null) return _certificate;

            var path = _config["Sri:CertificatePath"];
            var password = _config["Sri:CertificatePassword"];

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new InvalidOperationException("No se encontró el archivo de firma electrónica (.p12). Configure Sri:CertificatePath.");
            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Falta la contraseña de la firma electrónica. Configure Sri:CertificatePassword.");

            // En macOS no existe EphemeralKeySet; en Linux (Render) evita escribir la clave en disco
            var flags = OperatingSystem.IsMacOS() ? X509KeyStorageFlags.Exportable : X509KeyStorageFlags.EphemeralKeySet;

            var (certBytes, formato) = ReadCertificateBytes(path);
            var collection = new X509Certificate2Collection();
            try
            {
                collection.Import(certBytes, password, flags);
            }
            catch (CryptographicException ex)
            {
                // Solo metadatos (tamaños y formato), nunca el contenido de la firma
                throw new InvalidOperationException(
                    $"No se pudo abrir la firma ({formato}; {certBytes.Length} bytes): {ex.Message}", ex);
            }

            var cert = collection.FirstOrDefault(c => c.HasPrivateKey)
                ?? throw new InvalidOperationException("El archivo .p12 no contiene un certificado con clave privada.");

            if (cert.NotAfter < DateTime.Now)
                throw new InvalidOperationException($"La firma electrónica venció el {cert.NotAfter:dd/MM/yyyy}.");

            _certificate = cert;
            return cert;
        }
    }

    /// <summary>
    /// Lee el .p12 binario o, si el archivo es texto base64 (p. ej. un Secret File de Render,
    /// que solo admite texto), lo decodifica. Tolera BOM, saltos de línea y comillas alrededor.
    /// </summary>
    private static (byte[] Bytes, string Formato) ReadCertificateBytes(string path)
    {
        var bytes = File.ReadAllBytes(path);

        // Un PKCS#12 binario empieza con una SEQUENCE DER (0x30)
        if (bytes.Length > 0 && bytes[0] == 0x30)
            return (bytes, $"binario, archivo de {bytes.Length} bytes");

        var text = System.Text.Encoding.UTF8.GetString(bytes);
        var compact = new string(text.Where(c => !char.IsWhiteSpace(c) && c != '\uFEFF' && c != '"' && c != '\'').ToArray());
        var invalidos = compact.Count(c => !(char.IsAsciiLetterOrDigit(c) || c is '+' or '/' or '='));
        // Un .p12 en base64 empieza con "MII"; no se muestra el texto por si se pegó otra cosa (una contraseña)
        var descripcion = $"texto de {compact.Length} caracteres, {(compact.StartsWith("MII") ? "empieza como una firma" : "NO empieza como una firma (MII...)")}";

        if (invalidos > 0)
            return (bytes, $"{descripcion}, {invalidos} caracteres no válidos en base64");

        try
        {
            return (Convert.FromBase64String(compact), $"base64, {descripcion}");
        }
        catch (FormatException)
        {
            return (bytes, $"{descripcion}, base64 incompleto (la longitud debe ser múltiplo de 4)");
        }
    }
}
