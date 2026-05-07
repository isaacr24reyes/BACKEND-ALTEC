using AltecSystem.Application.Commands.Mundial;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Mundial;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AltecSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MundialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMundialRepository _mundialRepository;

    public MundialController(IMediator mediator, IMundialRepository mundialRepository)
    {
        _mediator = mediator;
        _mundialRepository = mundialRepository;
    }

    // ── Codes ─────────────────────────────────────────────────────────────────

    // POST api/Mundial/codes
    [HttpPost("codes")]
    public async Task<IActionResult> SaveCode([FromBody] SaveMundialCodeCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var id = await _mediator.Send(command);
            return Ok(new { id, message = "Código guardado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "Error al guardar el código." });
        }
    }

    // GET api/Mundial/codes
    [HttpGet("codes")]
    public async Task<IActionResult> GetCodes()
    {
        try
        {
            var result = await _mediator.Send(new GetMundialCodesQuery());
            return Ok(result);
        }
        catch
        {
            return StatusCode(500, new { error = "Error al obtener los códigos." });
        }
    }

    // GET api/Mundial/codes/validate/{codigo}
    [HttpGet("codes/validate/{codigo}")]
    public async Task<IActionResult> ValidateCode(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest(new { valid = false, message = "Código inválido." });

        try
        {
            var (exists, alreadyUsed) = await _mundialRepository.ValidateCodeAsync(codigo);

            if (!exists)
                return Ok(new { valid = false, message = "Código no encontrado." });

            if (alreadyUsed)
                return Ok(new { valid = false, message = "Este código ya fue utilizado." });

            return Ok(new { valid = true, message = "¡Código válido!" });
        }
        catch
        {
            return StatusCode(500, new { valid = false, message = "Error al validar el código." });
        }
    }

    // ── Pronosticos ───────────────────────────────────────────────────────────

    // POST api/Mundial/pronosticos
    [HttpPost("pronosticos")]
    public async Task<IActionResult> SavePronostico([FromBody] SavePronosticoCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var id = await _mediator.Send(command);
            return Ok(new { id, message = "¡Pronóstico registrado exitosamente!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "Error al registrar el pronóstico." });
        }
    }

    // GET api/Mundial/pronosticos
    [HttpGet("pronosticos")]
    public async Task<IActionResult> GetPronosticos()
    {
        try
        {
            var result = await _mediator.Send(new GetPronosticosQuery());
            return Ok(result);
        }
        catch
        {
            return StatusCode(500, new { error = "Error al obtener los pronósticos." });
        }
    }
}
