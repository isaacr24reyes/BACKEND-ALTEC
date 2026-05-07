using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Infrastructure.Repositories;

public class MundialRepository : IMundialRepository
{
    private readonly AltecSystemDbContext _context;

    public MundialRepository(AltecSystemDbContext context)
    {
        _context = context;
    }

    // ── Codes ─────────────────────────────────────────────────────────────────

    public async Task AddCodeAsync(MundialCode code)
    {
        await _context.MundialCodes.AddAsync(code);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MundialCode>> GetAllCodesAsync()
    {
        return await _context.MundialCodes
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string codigo)
    {
        return await _context.MundialCodes
            .AnyAsync(c => c.Codigo == codigo.ToUpperInvariant());
    }

    // ── Validate ──────────────────────────────────────────────────────────────

    public async Task<(bool exists, bool alreadyUsed)> ValidateCodeAsync(string codigo)
    {
        var upper = codigo.ToUpperInvariant();

        var exists = await _context.MundialCodes
            .AnyAsync(c => c.Codigo == upper);

        if (!exists)
            return (false, false);

        var alreadyUsed = await _context.MundialPronosticos
            .AnyAsync(p => p.CodigoUnico == upper);

        return (true, alreadyUsed);
    }

    // ── Pronosticos ───────────────────────────────────────────────────────────

    public async Task AddPronosticoAsync(MundialPronostico pronostico)
    {
        await _context.MundialPronosticos.AddAsync(pronostico);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MundialPronostico>> GetAllPronosticosAsync()
    {
        return await _context.MundialPronosticos
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> PronosticoExistsForCodeAsync(string codigoUnico)
    {
        return await _context.MundialPronosticos
            .AnyAsync(p => p.CodigoUnico == codigoUnico.ToUpperInvariant());
    }
}
