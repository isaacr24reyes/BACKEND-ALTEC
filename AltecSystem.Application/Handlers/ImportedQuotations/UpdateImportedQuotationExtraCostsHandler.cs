using AltecSystem.Application.Commands.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.ImportedQuotations;

public class UpdateImportedQuotationExtraCostsHandler : IRequestHandler<UpdateImportedQuotationExtraCostsCommand, bool>
{
    private const int MaxNota = 300;
    private readonly IImportedQuotationRepository _repository;

    public UpdateImportedQuotationExtraCostsHandler(IImportedQuotationRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(UpdateImportedQuotationExtraCostsCommand request, CancellationToken cancellationToken)
    {
        if (request.CostosExtras < 0)
            throw new InvalidOperationException("Los costos extras no pueden ser negativos.");
        if (request.CostosExtras > 10_000_000m)
            throw new InvalidOperationException("El monto de costos extras es demasiado alto.");

        var nota = string.IsNullOrWhiteSpace(request.Nota) ? null : request.Nota.Trim();
        if (nota?.Length > MaxNota)
            nota = nota[..MaxNota];

        return _repository.UpdateExtraCostsAsync(request.Id, Math.Round(request.CostosExtras, 4), nota, cancellationToken);
    }
}
