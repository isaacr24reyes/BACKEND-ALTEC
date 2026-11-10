using System.IO.Compression;
using System.Xml.Linq;
using AltecSystem.Application.Interfaces;

namespace AltecSystem.Infrastructure.Services;

/// <summary>
/// Saca las fotos ancladas en la primera hoja del .xlsx (xl/drawings) sin dependencias externas:
/// hoja → dibujo → anclas (fila de inicio) → imagen en xl/media.
/// </summary>
public class QuotationImageExtractor : IQuotationImageExtractor
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace Xdr = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";

    public Dictionary<int, QuotationImage> Extract(byte[] xlsx)
    {
        var result = new Dictionary<int, QuotationImage>();
        try
        {
            using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);

            var sheetPath = ResolveFirstSheetPath(zip);
            var drawingPath = RelationshipTargets(zip, sheetPath)
                .FirstOrDefault(t => t.Type.EndsWith("/drawing")).Path;
            if (drawingPath == null) return result;

            var drawing = LoadXml(zip, drawingPath);
            if (drawing == null) return result;
            var images = RelationshipTargets(zip, drawingPath).ToDictionary(t => t.Id, t => t.Path);

            foreach (var anchor in drawing.Root!.Elements()
                         .Where(e => e.Name == Xdr + "twoCellAnchor" || e.Name == Xdr + "oneCellAnchor"))
            {
                var rowText = anchor.Element(Xdr + "from")?.Element(Xdr + "row")?.Value;
                var embed = anchor.Descendants(A + "blip").FirstOrDefault()?.Attribute(Rel + "embed")?.Value;
                if (!int.TryParse(rowText, out var row0) || embed == null) continue;

                var fila = row0 + 1; // las anclas cuentan desde 0; las filas del Excel desde 1
                if (result.ContainsKey(fila) || !images.TryGetValue(embed, out var mediaPath)) continue;

                var entry = zip.GetEntry(mediaPath);
                var contentType = ContentTypeFor(mediaPath);
                if (entry == null || contentType == null) continue;

                using var s = entry.Open();
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                result[fila] = new QuotationImage(ms.ToArray(), contentType);
            }
        }
        catch (InvalidDataException)
        {
            // No es un zip válido: sin fotos
        }
        return result;
    }

    // ── Paquete Open XML ─────────────────────────────────────────────────────

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
        var relId = workbook?.Descendants(Main + "sheet").FirstOrDefault()?.Attribute(Rel + "id")?.Value;
        if (relId == null) return fallback;
        return RelationshipTargets(zip, "xl/workbook.xml").FirstOrDefault(t => t.Id == relId).Path ?? fallback;
    }

    /// <summary>Relaciones de una parte, con la ruta destino ya resuelta dentro del zip.</summary>
    private static IEnumerable<(string Id, string Type, string? Path)> RelationshipTargets(ZipArchive zip, string partPath)
    {
        var dir = Path.GetDirectoryName(partPath)?.Replace('\\', '/') ?? "";
        var relsPath = $"{dir}/_rels/{Path.GetFileName(partPath)}.rels";
        var rels = LoadXml(zip, relsPath);
        if (rels == null) yield break;

        foreach (var r in rels.Descendants(PkgRel + "Relationship"))
        {
            var id = r.Attribute("Id")?.Value;
            var target = r.Attribute("Target")?.Value;
            if (id == null || string.IsNullOrEmpty(target) || r.Attribute("TargetMode")?.Value == "External") continue;
            yield return (id, r.Attribute("Type")?.Value ?? "", ResolvePath(dir, target));
        }
    }

    private static string ResolvePath(string baseDir, string target)
    {
        if (target.StartsWith('/')) return target.TrimStart('/');
        var parts = new List<string>(baseDir.Split('/', StringSplitOptions.RemoveEmptyEntries));
        foreach (var seg in target.Split('/'))
        {
            if (seg == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); }
            else if (seg != "." && seg.Length > 0) parts.Add(seg);
        }
        return string.Join('/', parts);
    }

    private static string? ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        _ => null // emf/wmf u otros formatos que el navegador no muestra
    };
}
