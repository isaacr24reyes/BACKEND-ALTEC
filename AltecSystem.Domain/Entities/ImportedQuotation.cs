namespace AltecSystem.Domain.Entities;

/// <summary>Excel de cotización importado al historial (se guarda el archivo original).</summary>
public class ImportedQuotation
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
    public int TotalItems { get; set; }
    public decimal TotalMonto { get; set; }
    /// <summary>Gastos para que la mercadería llegue a Ecuador (aduana, IVA, Alibaba, impuesto TC...).</summary>
    public decimal CostosExtras { get; set; }
    public string? CostosExtrasNota { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public List<ImportedQuotationItem> Items { get; set; } = new();
}
