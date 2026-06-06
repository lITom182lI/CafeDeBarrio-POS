using CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
using CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
using CafeBarrio.Application.Features.Turnos.Queries.GetTurnoActivo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/turnos")]
[Authorize]
public class TurnosController : ControllerBase
{
    private readonly IMediator _mediator;
    public TurnosController(IMediator mediator) => _mediator = mediator;

    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo([FromQuery] int sedeId)
    {
        var result = await _mediator.Send(new GetTurnoActivoQuery(sedeId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost("abrir")]
    public async Task<IActionResult> Abrir([FromBody] AbrirTurnoCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { turnoId = result.Value }) : BadRequest(result.Errors);
    }

    [HttpPut("{id}/cerrar")]
    public async Task<IActionResult> Cerrar(int id, [FromBody] CerrarTurnoRequest request)
    {
        var result = await _mediator.Send(new CerrarTurnoCommand(id, request.MontoEfectivoCierto, request.Observaciones));
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}

public record CerrarTurnoRequest(decimal MontoEfectivoCierto, string? Observaciones);
