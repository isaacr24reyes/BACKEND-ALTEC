namespace AltecSystem.Application.Interfaces;

/// <summary>Imagen incrustada en el Excel importado.</summary>
public record QuotationImage(byte[] Bytes, string ContentType);

public interface IQuotationImageExtractor
{
    /// <summary>
    /// Fotos de la primera hoja por nº de fila del Excel (la fila donde está anclada cada imagen).
    /// Si una fila tiene varias, se toma la primera. Nunca lanza: un archivo sin fotos devuelve vacío.
    /// </summary>
    Dictionary<int, QuotationImage> Extract(byte[] xlsx);
}
