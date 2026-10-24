using AltecSystem.Application.DTOs.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Queries.ImportedQuotations;

public record GetImportedQuotationDetailQuery(Guid Id) : IRequest<ImportedQuotationDetailDto?>;
