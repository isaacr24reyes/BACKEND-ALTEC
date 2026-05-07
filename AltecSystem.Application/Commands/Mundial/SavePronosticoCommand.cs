using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AltecSystem.Application.Commands.Mundial;

public class SavePronosticoCommand : IRequest<int>
{
    [Required] public string CodigoUnico    { get; set; } = string.Empty;
    [Required] public string Nombre         { get; set; } = string.Empty;
    [Required] public string Telefono       { get; set; } = string.Empty;
    [Required] public string Campeon        { get; set; } = string.Empty;
    [Required] public string Subcampeon     { get; set; } = string.Empty;
    [Required] public string TercerLugar    { get; set; } = string.Empty;
    [Required] public string CuartoLugar    { get; set; } = string.Empty;
    [Required] public string Goleador       { get; set; } = string.Empty;
    [Required] public string ResultadoFinal { get; set; } = string.Empty;
}
