using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

public interface IMundialRepository
{
    // Codes
    Task AddCodeAsync(MundialCode code);
    Task<List<MundialCode>> GetAllCodesAsync();
    Task<bool> CodeExistsAsync(string codigo);

    // Validate (code exists AND not yet used)
    Task<(bool exists, bool alreadyUsed)> ValidateCodeAsync(string codigo);

    // Pronosticos
    Task AddPronosticoAsync(MundialPronostico pronostico);
    Task<List<MundialPronostico>> GetAllPronosticosAsync();
    Task<bool> PronosticoExistsForCodeAsync(string codigoUnico);
}
