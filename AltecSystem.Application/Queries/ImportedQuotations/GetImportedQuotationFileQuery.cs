using AltecSystem.Application.DTOs.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Queries.ImportedQuotations;

public record GetImportedQuotationFileQuery(Guid Id) : IRequest<ImportedQuotationFileDto?>;
