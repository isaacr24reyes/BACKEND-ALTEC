using AltecSystem.Application.DTOs.Mundial;
using MediatR;

namespace AltecSystem.Application.Queries.Mundial;

public class GetMundialCodesQuery : IRequest<List<MundialCodeDto>>
{
}
