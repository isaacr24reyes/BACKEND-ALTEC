using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AltecSystem.Application.Commands.Mundial;

public class SaveMundialCodeCommand : IRequest<int>
{
    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public string CreatedBy { get; set; } = string.Empty;
}
