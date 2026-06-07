using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Auth.Dtos;
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
    private readonly IUnitOfWork _uow;

    public AuthController(
        IUsuarioRepository usuarios, IJwtService jwt,
        IPasswordHasher hasher, IUnitOfWork uow)
    {
        _usuarios = usuarios; _jwt = jwt; _hasher = hasher; _uow = uow;
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

        if (request.NewPassword.Length < 8)
            return BadRequest(new { message = "La nueva contraseña debe tener al menos 8 caracteres." });

        var usuario = await _usuarios.GetByEmailAsync(email, ct);
        if (usuario is null)
            return NotFound(new { message = "Usuario no encontrado." });

        if (!_hasher.Verify(request.CurrentPassword, usuario.PasswordHash))
            return BadRequest(new { message = "La contraseña actual es incorrecta." });

        usuario.PasswordHash = _hasher.Hash(request.NewPassword);
        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
