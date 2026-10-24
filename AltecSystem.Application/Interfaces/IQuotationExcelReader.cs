using AltecSystem.Application.DTOs.ImportedQuotations;

namespace AltecSystem.Application.Interfaces;

public interface IQuotationExcelReader
{
    /// <summary>
    /// Lee las filas de producto de un .xlsx con encabezados Producto / QTY / PRICE / Total.
    /// Lanza InvalidOperationException si el formato no es válido.
    /// </summary>
    List<QuotationExcelRow> Read(Stream xlsx);
}
