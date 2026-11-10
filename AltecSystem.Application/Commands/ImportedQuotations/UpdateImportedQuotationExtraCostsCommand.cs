using MediatR;

namespace AltecSystem.Application.Commands.ImportedQuotations;

/// <summary>Guarda (o cambia) los costos extras de una importación. Devuelve false si no existe.</summary>
public record UpdateImportedQuotationExtraCostsCommand(Guid Id, decimal CostosExtras, string? Nota) : IRequest<bool>;

/// <summary>Cuerpo del PUT api/cotizaciones-importadas/{id}/costos-extras.</summary>
public class UpdateExtraCostsRequest
{
    public decimal CostosExtras { get; set; }
    public string? Nota { get; set; }
}
