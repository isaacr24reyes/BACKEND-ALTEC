using AltecSystem.Application.Commands.User;
using AltecSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MediatR;
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
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

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var command = new GetUserByUsernameCommand(username);
        var userDto = await _mediator.Send(command);

        if (userDto == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(userDto);
    }
}