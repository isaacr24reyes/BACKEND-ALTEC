namespace AltecSystem.Application.DTOs.ImportedQuotations;

/// <summary>Fila leída de un Excel de cotización.</summary>
public record QuotationExcelRow(int Fila, string Producto, decimal Cantidad, decimal PrecioUnitario, decimal Total);
