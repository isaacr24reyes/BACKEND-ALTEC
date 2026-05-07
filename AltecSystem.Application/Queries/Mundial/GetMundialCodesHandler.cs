using AltecSystem.Application.DTOs.Mundial;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Queries.Mundial;

public class GetMundialCodesHandler : IRequestHandler<GetMundialCodesQuery, List<MundialCodeDto>>
{
    private readonly IMundialRepository _mundialRepository;

    public GetMundialCodesHandler(IMundialRepository mundialRepository)
    {
        _mundialRepository = mundialRepository;
    }

    public async Task<List<MundialCodeDto>> Handle(GetMundialCodesQuery request, CancellationToken cancellationToken)
    {
        var codes = await _mundialRepository.GetAllCodesAsync();

        return codes.Select(c => new MundialCodeDto
        {
            Id        = c.Id,
            Codigo    = c.Codigo,
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}
