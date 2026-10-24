using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;

namespace AltecSystem.Infrastructure.Services;

/// <summary>
/// Lector mínimo de .xlsx (Open XML) sin dependencias externas.
/// Busca la fila de encabezados (Producto / QTY / PRICE / Total) en la primera hoja
/// y devuelve las filas de producto que vienen debajo. Las imágenes se ignoran.
/// </summary>
public class QuotationExcelReader : IQuotationExcelReader
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";

    private static readonly string[] ProductHeaders = { "producto", "product", "descripcion", "description", "articulo", "item" };
    private static readonly string[] QtyHeaders = { "qty", "cantidad", "quantity", "cant" };
    private static readonly string[] PriceHeaders = { "price", "precio", "unit price", "precio unitario", "p. unit", "p.unit", "pvp" };
    private static readonly string[] TotalHeaders = { "total", "subtotal", "importe" };

    public List<QuotationExcelRow> Read(Stream xlsx)
    {
        ZipArchive zip;
        try
        {
            zip = new ZipArchive(xlsx, ZipArchiveMode.Read, leaveOpen: true);
        }
        catch (InvalidDataException)
        {
            throw new InvalidOperationException("El archivo no es un Excel .xlsx válido.");
        }

        using (zip)
        {
            var sharedStrings = ReadSharedStrings(zip);
            var sheet = LoadXml(zip, ResolveFirstSheetPath(zip))
                        ?? throw new InvalidOperationException("No se encontró ninguna hoja en el Excel.");

            var rows = sheet.Descendants(Main + "sheetData").Elements(Main + "row")
                .Select(r => ReadRow(r, sharedStrings))
                .ToList();

            var header = rows.FirstOrDefault(r => FindColumn(r.Cells, ProductHeaders) != null
                                                  && FindColumn(r.Cells, QtyHeaders) != null
                                                  && FindColumn(r.Cells, PriceHeaders) != null);
            if (header == null)
                throw new InvalidOperationException(
                    "No se encontró la fila de encabezados. El Excel debe tener las columnas Producto, QTY y PRICE (y opcionalmente Total).");

            var colProduct = FindColumn(header.Cells, ProductHeaders)!.Value;
            var colQty = FindColumn(header.Cells, QtyHeaders)!.Value;
            var colPrice = FindColumn(header.Cells, PriceHeaders)!.Value;
            var colTotal = FindColumn(header.Cells, TotalHeaders);

            var result = new List<QuotationExcelRow>();
            foreach (var row in rows.Where(r => r.Number > header.Number))
            {
                var producto = Normalize(row.Get(colProduct));
                if (string.IsNullOrEmpty(producto))
                    continue; // filas vacías y la fila "TOTAL:" (no tiene producto)

                var cantidad = ParseDecimal(row.Get(colQty));
                var precio = ParseDecimal(row.Get(colPrice));
                if (cantidad == null && precio == null)
                    continue; // texto suelto sin datos numéricos

                var total = colTotal.HasValue ? ParseDecimal(row.Get(colTotal.Value)) : null;
                var qty = cantidad ?? 0m;
                var price = precio ?? 0m;

                result.Add(new QuotationExcelRow(
                    row.Number,
                    producto,
                    Math.Round(qty, 4),
                    Math.Round(price, 6),
                    Math.Round(total ?? qty * price, 6)));
            }

            return result;
        }
    }

    // ── Lectura de partes del paquete ─────────────────────────────────────────

    private static XDocument? LoadXml(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path);
        if (entry == null) return null;
        using var s = entry.Open();
        return XDocument.Load(s);
    }

    private static string ResolveFirstSheetPath(ZipArchive zip)
    {
        const string fallback = "xl/worksheets/sheet1.xml";
        var workbook = LoadXml(zip, "xl/workbook.xml");
        var rels = LoadXml(zip, "xl/_rels/workbook.xml.rels");
        var relId = workbook?.Descendants(Main + "sheet").FirstOrDefault()?.Attribute(Rel + "id")?.Value;
        if (relId == null || rels == null) return fallback;

        var target = rels.Descendants(PkgRel + "Relationship")
            .FirstOrDefault(r => r.Attribute("Id")?.Value == relId)?.Attribute("Target")?.Value;
        if (string.IsNullOrEmpty(target)) return fallback;

        return target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
    }

    private static List<string> ReadSharedStrings(ZipArchive zip)
    {
        var doc = LoadXml(zip, "xl/sharedStrings.xml");
        if (doc == null) return new List<string>();
        return doc.Root!.Elements(Main + "si")
            .Select(si => string.Concat(si.Descendants(Main + "t").Select(t => t.Value)))
            .ToList();
    }

    private static SheetRow ReadRow(XElement row, List<string> sharedStrings)
    {
        var number = int.TryParse(row.Attribute("r")?.Value, out var n) ? n : 0;
        var cells = new Dictionary<int, string>();

        foreach (var c in row.Elements(Main + "c"))
        {
            var col = ColumnIndex(c.Attribute("r")?.Value);
            if (col < 0) continue;

            var type = c.Attribute("t")?.Value;
            string? value = type switch
            {
                "s" => int.TryParse(c.Element(Main + "v")?.Value, out var idx) && idx < sharedStrings.Count
                    ? sharedStrings[idx] : null,
                "inlineStr" => string.Concat(c.Descendants(Main + "t").Select(t => t.Value)),
                _ => c.Element(Main + "v")?.Value
            };

            if (value != null) cells[col] = value;
        }

        return new SheetRow(number, cells);
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    private static int ColumnIndex(string? cellRef)
    {
        if (string.IsNullOrEmpty(cellRef)) return -1;
        var index = 0;
        foreach (var ch in cellRef)
        {
            if (!char.IsLetter(ch)) break;
            index = index * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
        }
        return index - 1;
    }

    private static int? FindColumn(Dictionary<int, string> cells, string[] names)
    {
        foreach (var (col, text) in cells)
        {
            var key = RemoveAccents(Normalize(text)).ToLowerInvariant().TrimEnd(':');
            if (names.Contains(key)) return col;
        }
        return null;
    }

    private static decimal? ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var text = raw.Trim().Replace("$", "").Replace(" ", "");

        if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return d;
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var dbl))
            return (decimal)dbl;
        if (decimal.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out d))
            return d;
        return null;
    }

    private static string Normalize(string? text) =>
        string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string RemoveAccents(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record SheetRow(int Number, Dictionary<int, string> Cells)
    {
        public string? Get(int col) => Cells.TryGetValue(col, out var v) ? v : null;
    }
}
