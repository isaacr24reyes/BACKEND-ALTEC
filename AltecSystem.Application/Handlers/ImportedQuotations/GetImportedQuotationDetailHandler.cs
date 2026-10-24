using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Handlers.ImportedQuotations;

public class GetImportedQuotationDetailHandler : IRequestHandler<GetImportedQuotationDetailQuery, ImportedQuotationDetailDto?>
{
    private readonly IImportedQuotationRepository _repository;

    public GetImportedQuotationDetailHandler(IImportedQuotationRepository repository)
    {
        _repository = repository;
    }

    public Task<ImportedQuotationDetailDto?> Handle(GetImportedQuotationDetailQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetDetailAsync(request.Id, cancellationToken);
    }
}
