using AltecSystem.Application.Commands.Mundial;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.Mundial;

public class SaveMundialCodeHandler : IRequestHandler<SaveMundialCodeCommand, int>
{
    private readonly IMundialRepository _mundialRepository;

    public SaveMundialCodeHandler(IMundialRepository mundialRepository)
    {
        _mundialRepository = mundialRepository;
    }

    public async Task<int> Handle(SaveMundialCodeCommand request, CancellationToken cancellationToken)
    {
        var exists = await _mundialRepository.CodeExistsAsync(request.Codigo);
        if (exists)
            throw new InvalidOperationException($"El código '{request.Codigo}' ya existe.");

        TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        DateTimeOffset fechaEcuador = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ecuadorTimeZone);

        var mundialCode = new MundialCode
        {
            Codigo    = request.Codigo.ToUpperInvariant(),
            CreatedBy = request.CreatedBy,
            CreatedAt = fechaEcuador
        };

        await _mundialRepository.AddCodeAsync(mundialCode);
        return mundialCode.Id;
    }
}
