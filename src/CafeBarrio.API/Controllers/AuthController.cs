using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Auth.Dtos;
using CafeBarrio.Application.Features.Auth.Commands.ChangePassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _hasher;
    private readonly ISender _sender;

    public AuthController(
        IUsuarioRepository usuarios, IJwtService jwt,
        IPasswordHasher hasher, ISender sender)
    {
        _usuarios = usuarios; _jwt = jwt; _hasher = hasher; _sender = sender;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login-policy")]
    [ProducesResponseType<LoginResponse>(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null || !_hasher.Verify(request.Password, usuario.PasswordHash))
            return Unauthorized(new { message = "Credenciales incorrectas" });

        var token = _jwt.GenerateToken(usuario);
        return Ok(new LoginResponse(token, usuario.Rol, usuario.Email));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public IActionResult Me()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var rol   = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return Ok(new { email, rol });
    }

    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (email is null)
            return Unauthorized(new { message = "Token invalido." });

        var command = new ChangePasswordCommand(email, request.CurrentPassword, request.NewPassword);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            var error = result.Errors[0];
            return BadRequest(new { message = error.Message });
        }

        return NoContent();
    }
}
