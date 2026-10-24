using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.ImportedQuotations;
using MediatR;

namespace AltecSystem.Application.Handlers.ImportedQuotations;

public class GetImportedQuotationsHandler : IRequestHandler<GetImportedQuotationsQuery, List<ImportedQuotationSummaryDto>>
{
    private readonly IImportedQuotationRepository _repository;

    public GetImportedQuotationsHandler(IImportedQuotationRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ImportedQuotationSummaryDto>> Handle(GetImportedQuotationsQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetSummariesAsync(request.Desde?.Date, request.Hasta?.Date, request.Buscar, cancellationToken);
    }
}
