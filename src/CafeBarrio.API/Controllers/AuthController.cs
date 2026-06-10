using CafeBarrio.Application.Features.Auth.Dtos;
using CafeBarrio.Application.Features.Auth.Commands.ChangePassword;
using CafeBarrio.Application.Features.Auth.Commands.Login;
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
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login-policy")]
    [ProducesResponseType<LoginResponse>(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), ct);

        if (result.IsFailure)
        {
            return Unauthorized(new { message = result.Errors[0].Message });
        }

        return Ok(result.Value);
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
