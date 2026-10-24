using AltecSystem.Application.DTOs.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Queries.ImportedQuotations;

public class GetImportedQuotationsQuery : IRequest<List<ImportedQuotationSummaryDto>>
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Buscar { get; set; }
}
