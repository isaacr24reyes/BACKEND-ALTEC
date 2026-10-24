using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Handlers.ImportedQuotations;

public class GetImportedQuotationFileHandler : IRequestHandler<GetImportedQuotationFileQuery, ImportedQuotationFileDto?>
{
    private readonly IImportedQuotationRepository _repository;

    public GetImportedQuotationFileHandler(IImportedQuotationRepository repository)
    {
        _repository = repository;
    }

    public Task<ImportedQuotationFileDto?> Handle(GetImportedQuotationFileQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetFileAsync(request.Id, cancellationToken);
    }
}
