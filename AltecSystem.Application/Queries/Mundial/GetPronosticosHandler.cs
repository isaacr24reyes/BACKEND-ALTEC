using AltecSystem.Application.DTOs.Mundial;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Queries.Mundial;

public class GetPronosticosHandler : IRequestHandler<GetPronosticosQuery, List<MundialPronosticoDto>>
{
    private readonly IMundialRepository _mundialRepository;

    public GetPronosticosHandler(IMundialRepository mundialRepository)
    {
        _mundialRepository = mundialRepository;
    }

    public async Task<List<MundialPronosticoDto>> Handle(GetPronosticosQuery request, CancellationToken cancellationToken)
    {
        var pronosticos = await _mundialRepository.GetAllPronosticosAsync();

        return pronosticos.Select(p => new MundialPronosticoDto
        {
            Id             = p.Id,
            CodigoUnico    = p.CodigoUnico,
            Nombre         = p.Nombre,
            Campeon        = p.Campeon,
            Subcampeon     = p.Subcampeon,
            TercerLugar    = p.TercerLugar,
            CuartoLugar    = p.CuartoLugar,
            Goleador       = p.Goleador,
            ResultadoFinal = p.ResultadoFinal,
            CreatedAt      = p.CreatedAt
        }).ToList();
    }
}
