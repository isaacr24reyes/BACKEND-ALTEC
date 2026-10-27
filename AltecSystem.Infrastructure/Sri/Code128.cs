using System.Globalization;
using System.Text;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// Código de barras Code 128 (el que usa el RIDE del SRI para la clave de acceso), generado como SVG.
/// Usa el subconjunto C (pares de dígitos) y, si queda un dígito suelto al final, cambia al subconjunto B.
/// </summary>
public static class Code128
{
    // Anchos de barra/espacio de cada símbolo (valores 0..106; el 106 es STOP)
    private static readonly string[] Patterns =
    {
        "212222","222122","222221","121223","121322","131222","122213","122312","132212","221213",
        "221312","231212","112232","122132","122231","113222","123122","123221","223211","221132",
        "221231","213212","223112","312131","311222","321122","321221","312212","322112","322211",
        "212123","212321","232121","111323","131123","131321","112313","132113","132311","211313",
        "231113","231311","112133","112331","132131","113123","113321","133121","313121","211331",
        "231131","213113","213311","213131","311123","311321","331121","312113","312311","332111",
        "314111","221411","431111","111224","111422","121124","121421","141122","141221","112214",
        "112412","122114","122411","142112","142211","241211","221114","413111","241112","134111",
        "111242","121142","121241","114212","124112","124211","411212","421112","421211","212141",
        "214121","412121","111143","111341","131141","114113","114311","411113","411311","113141",
        "114131","311141","411131","211412","211214","211232","2331112"
    };

    private const int StartC = 105;
    private const int CodeB = 100;
    private const int Stop = 106;

    /// <summary>Valores de los símbolos (incluye start, checksum y stop) para un texto solo numérico.</summary>
    public static List<int> Encode(string digits)
    {
        if (string.IsNullOrEmpty(digits) || !digits.All(char.IsDigit))
            throw new ArgumentException("Code128 (subconjunto C) requiere solo dígitos.", nameof(digits));

        var values = new List<int> { StartC };
        var i = 0;
        for (; i + 1 < digits.Length; i += 2)
            values.Add((digits[i] - '0') * 10 + (digits[i + 1] - '0'));

        if (i < digits.Length)
        {
            values.Add(CodeB);
            values.Add(digits[i] - ' '); // subconjunto B: valor = ASCII - 32
        }

        var checksum = values[0];
        for (var p = 1; p < values.Count; p++)
            checksum += values[p] * p;
        values.Add(checksum % 103);
        values.Add(Stop);
        return values;
    }

    /// <summary>SVG con las barras (sin texto), escalable al ancho que se necesite.</summary>
    public static string ToSvg(string digits, int height = 60)
    {
        var modules = new StringBuilder();
        foreach (var v in Encode(digits))
            modules.Append(Patterns[v]);

        const int quiet = 10; // zona en blanco a cada lado
        var x = quiet;
        var bars = new StringBuilder();
        var black = true;
        foreach (var w in modules.ToString().Select(c => c - '0'))
        {
            if (black)
                bars.Append(CultureInfo.InvariantCulture, $"<rect x=\"{x}\" y=\"0\" width=\"{w}\" height=\"{height}\"/>");
            x += w;
            black = !black;
        }

        var width = x + quiet;
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" preserveAspectRatio=\"none\">" +
               $"<rect width=\"{width}\" height=\"{height}\" fill=\"#fff\"/><g fill=\"#000\">{bars}</g></svg>";
    }
}
