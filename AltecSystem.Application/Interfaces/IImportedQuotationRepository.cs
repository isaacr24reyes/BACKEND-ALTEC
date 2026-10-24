using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

public interface IImportedQuotationRepository
{
    Task AddAsync(ImportedQuotation quotation, CancellationToken ct = default);
    Task<List<ImportedQuotationSummaryDto>> GetSummariesAsync(DateTime? desde, DateTime? hasta, string? buscar, CancellationToken ct = default);
    Task<ImportedQuotationDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<ImportedQuotationFileDto?> GetFileAsync(Guid id, CancellationToken ct = default);
}
