using AltecSystem.Application.Commands.User;
using AltecSystem.Application.DTOs.Auth;
using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.User;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public AuthController(IAuthService authService, IMediator mediator)
    {
        _authService = authService;
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var token = await _authService.AuthenticateAsync(loginDto.Username, loginDto.Password);
        if (token == null)
            return Unauthorized(new { message = "Credenciales incorrectas" });

        return Ok(new {token});
    }
    [HttpGet("by-username")]
    public async Task<ActionResult<UserDetailsDto>> GetByUsername([FromQuery] string username)
    {
        var result = await _mediator.Send(new GetUserByUsernameQuery(username));

        if (result == null)
            return NotFound("Usuario no encontrado.");

        return Ok(result);
    }
    /// <summary>
    /// Busca usuarios por coincidencia parcial en Username o Name (case-insensitive).
    /// Ej: q=is -> Isaac, isabella, cris, etc.
    /// Soporta paginación opcional.
    /// </summary>
    [HttpGet("by-name")]
    public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAllByName(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Debes enviar el parámetro 'q' con el término de búsqueda.");

        if (page <= 0 || pageSize <= 0) 
            return BadRequest("Parámetros de paginación inválidos.");

        var result = await _mediator.Send(new GetUsersByNameQuery(q, page, pageSize));
        return Ok(result);
    }
    [HttpPost("add-points")]
    public async Task<ActionResult<UserDetailsDto>> AddPoints([FromBody] AddPointsRequestDto request)
    {
        var result = await _mediator.Send(new AddPointsCommand(request.Name, request.Points));
        return Ok(result);
    }
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersCommand(), ct);
        return Ok(result);
    }
    // 🔹 Crear nuevo usuario
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var success = await _mediator.Send(command, ct);

        if (!success)
            return BadRequest(new { message = "No se pudo crear el usuario" });

        return Ok(new { message = "✅ Usuario creado con éxito" });
    }
    
}