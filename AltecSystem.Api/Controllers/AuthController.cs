using AltecSystem.Application.Commands.User;
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
    
}