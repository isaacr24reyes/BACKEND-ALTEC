using AltecSystem.Application.Commands.ImportedQuotations;
using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.ImportedQuotations;

public class UploadImportedQuotationHandler : IRequestHandler<UploadImportedQuotationCommand, ImportedQuotationSummaryDto>
{
    public const long MaxFileBytes = 50L * 1024 * 1024; // 50 MB

    private readonly IImportedQuotationRepository _repository;
    private readonly IQuotationExcelReader _excelReader;

    public UploadImportedQuotationHandler(IImportedQuotationRepository repository, IQuotationExcelReader excelReader)
    {
        _repository = repository;
        _excelReader = excelReader;
    }

    public async Task<ImportedQuotationSummaryDto> Handle(UploadImportedQuotationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre es obligatorio.");

        var file = request.Archivo;
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Debes adjuntar el archivo Excel.");

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Solo se permiten archivos Excel .xlsx.");

        if (file.Length > MaxFileBytes)
            throw new InvalidOperationException("El archivo supera el máximo de 50 MB.");

        // Copia exacta del archivo subido (con fotos y formato) para poder descargarlo igual
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }

        List<QuotationExcelRow> rows;
        using (var readStream = new MemoryStream(bytes, writable: false))
        {
            rows = _excelReader.Read(readStream);
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("El Excel no tiene productos para importar.");

        TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        DateTimeOffset fechaEcuador = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ecuadorTimeZone);

        var quotation = new ImportedQuotation
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Fecha = request.Fecha.Date,
            NombreArchivo = Path.GetFileName(file.FileName),
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : file.ContentType,
            TamanoBytes = bytes.LongLength,
            Archivo = bytes,
            TotalItems = rows.Count,
            TotalMonto = Math.Round(rows.Sum(r => r.Total), 4),
            CreatedAt = fechaEcuador,
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "ADMIN" : request.CreatedBy.Trim()
        };

        quotation.Items = rows.Select(r => new ImportedQuotationItem
        {
            Id = Guid.NewGuid(),
            ImportedQuotationId = quotation.Id,
            Fila = r.Fila,
            Producto = r.Producto.Length > 500 ? r.Producto[..500] : r.Producto,
            Cantidad = r.Cantidad,
            PrecioUnitario = r.PrecioUnitario,
            Total = r.Total
        }).ToList();

        await _repository.AddAsync(quotation, cancellationToken);

        return new ImportedQuotationSummaryDto
        {
            Id = quotation.Id,
            Nombre = quotation.Nombre,
            Fecha = quotation.Fecha,
            NombreArchivo = quotation.NombreArchivo,
            TamanoBytes = quotation.TamanoBytes,
            TotalItems = quotation.TotalItems,
            TotalMonto = quotation.TotalMonto,
            CreatedAt = quotation.CreatedAt,
            CreatedBy = quotation.CreatedBy
        };
    }
}
