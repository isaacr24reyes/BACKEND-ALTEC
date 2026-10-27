using System.Security.Cryptography;

namespace AltecSystem.Application.Services.Sri;

/// <summary>Clave de acceso de 49 dígitos del SRI (ficha técnica de comprobantes electrónicos, esquema offline).</summary>
public static class SriAccessKey
{
    /// <param name="fechaEmision">Fecha de emisión (ddMMyyyy).</param>
    /// <param name="codDoc">Tipo de comprobante: 01 factura.</param>
    public static string Generate(DateTime fechaEmision, string codDoc, string ruc, byte ambiente,
        string estab, string ptoEmi, int secuencial, string? codigoNumerico = null)
    {
        codigoNumerico ??= RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");

        var baseKey = string.Concat(
            fechaEmision.ToString("ddMMyyyy"),
            codDoc,
            ruc,
            ambiente.ToString(),
            estab,
            ptoEmi,
            secuencial.ToString("D9"),
            codigoNumerico,
            "1"); // tipo de emisión: normal

        if (baseKey.Length != 48 || !baseKey.All(char.IsDigit))
            throw new InvalidOperationException($"Clave de acceso inválida ({baseKey.Length} dígitos): revise RUC, establecimiento y punto de emisión.");

        return baseKey + CheckDigit(baseKey);
    }

    /// <summary>Dígito verificador módulo 11 (pesos 2..7 de derecha a izquierda; 11 → 0, 10 → 1).</summary>
    public static int CheckDigit(string digits)
    {
        var sum = 0;
        var weight = 2;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            sum += (digits[i] - '0') * weight;
            weight = weight == 7 ? 2 : weight + 1;
        }

        var dv = 11 - sum % 11;
        return dv switch { 11 => 0, 10 => 1, _ => dv };
    }
}
