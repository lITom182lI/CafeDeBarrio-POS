using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
using CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
using CafeBarrio.Application.Features.Turnos.Queries.GetTurnoActivo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/turnos")]
[Authorize]
[Produces("application/json")]
public class TurnosController : ControllerBase
{
    private readonly IMediator _mediator;
    public TurnosController(IMediator mediator) => _mediator = mediator;

    [HttpGet("activo")]
    [ProducesResponseType<TurnoActivoDto>(200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetActivo([FromQuery] int sedeId)
    {
        var result = await _mediator.Send(new GetTurnoActivoQuery(sedeId));
        if (!result.IsSuccess) return BadRequest(result.Errors);
        return result.Value is null ? NoContent() : Ok(result.Value);
    }

    [HttpPost("abrir")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Abrir([FromBody] AbrirTurnoCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { turnoId = result.Value }) : BadRequest(result.Errors);
    }

    [HttpPut("{id}/cerrar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cerrar(int id, [FromBody] CerrarTurnoRequest request)
    {
        var result = await _mediator.Send(
            new CerrarTurnoCommand(id, request.MontoEfectivoCierto, request.Observaciones));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}

public record CerrarTurnoRequest(decimal MontoEfectivoCierto, string? Observaciones);
