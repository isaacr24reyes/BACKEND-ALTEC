using AltecSystem.Application.Commands.Mundial;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.Mundial;

public class SavePronosticoHandler : IRequestHandler<SavePronosticoCommand, int>
{
    private readonly IMundialRepository _mundialRepository;

    public SavePronosticoHandler(IMundialRepository mundialRepository)
    {
        _mundialRepository = mundialRepository;
    }

    public async Task<int> Handle(SavePronosticoCommand request, CancellationToken cancellationToken)
    {
        var upper = request.CodigoUnico.ToUpperInvariant();

        var (exists, alreadyUsed) = await _mundialRepository.ValidateCodeAsync(upper);

        if (!exists)
            throw new KeyNotFoundException("El código de participación no existe.");

        if (alreadyUsed)
            throw new InvalidOperationException("Este código ya fue utilizado para registrar un pronóstico.");

        TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        DateTimeOffset fechaEcuador = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ecuadorTimeZone);

        var pronostico = new MundialPronostico
        {
            CodigoUnico    = upper,
            Nombre         = request.Nombre.Trim(),
            Telefono       = request.Telefono.Trim(),
            Campeon        = request.Campeon.Trim(),
            Subcampeon     = request.Subcampeon.Trim(),
            TercerLugar    = request.TercerLugar.Trim(),
            CuartoLugar    = request.CuartoLugar.Trim(),
            Goleador       = request.Goleador.Trim(),
            ResultadoFinal = request.ResultadoFinal.Trim(),
            CreatedAt      = fechaEcuador
        };

        await _mundialRepository.AddPronosticoAsync(pronostico);
        return pronostico.Id;
    }
}
