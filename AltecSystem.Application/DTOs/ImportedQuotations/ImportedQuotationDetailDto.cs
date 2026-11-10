namespace AltecSystem.Application.DTOs.ImportedQuotations;

public class ImportedQuotationDetailDto : ImportedQuotationSummaryDto
{
    public List<ImportedQuotationItemDto> Items { get; set; } = new();

    /// <summary>Recargo aplicado a cada producto: CostosExtras / valor total de la mercadería (%).</summary>
    public decimal PorcentajeRecargo { get; set; }

    /// <summary>Valor en China + costos extras.</summary>
    public decimal TotalEcuador { get; set; }
}

public class ImportedQuotationItemDto
{
    public int Fila { get; set; }
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }

    /// <summary>Costo unitario puesto en Ecuador (costo en China + su parte de los costos extras).</summary>
    public decimal CostoEcuadorUnitario { get; set; }
    public decimal CostoEcuadorTotal { get; set; }
}
