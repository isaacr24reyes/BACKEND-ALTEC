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

            var collection = new X509Certificate2Collection();
            collection.Import(ReadCertificateBytes(path), password, flags);

            var cert = collection.FirstOrDefault(c => c.HasPrivateKey)
                ?? throw new InvalidOperationException("El archivo .p12 no contiene un certificado con clave privada.");

            if (cert.NotAfter < DateTime.Now)
                throw new InvalidOperationException($"La firma electrónica venció el {cert.NotAfter:dd/MM/yyyy}.");

            _certificate = cert;
            return cert;
        }
    }

    /// <summary>
    /// Lee el .p12 tal cual o, si el archivo es texto base64 (p. ej. un Secret File de Render,
    /// que solo admite texto), lo decodifica.
    /// </summary>
    private static byte[] ReadCertificateBytes(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var compact = string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
        if (compact.Length > 0 && compact.All(c => char.IsLetterOrDigit(c) || c is '+' or '/' or '='))
        {
            try { return Convert.FromBase64String(compact); }
            catch (FormatException) { /* no era base64: se usa el binario */ }
        }
        return bytes;
    }
}
