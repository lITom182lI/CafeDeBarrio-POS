using CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;
using CafeBarrio.Application.Features.MovimientosCaja.Dtos;
using CafeBarrio.Application.Features.MovimientosCaja.Queries.GetMovimientosByTurno;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/movimientos-caja")]
[Authorize]
[Produces("application/json")]
public class MovimientosCajaController : ControllerBase
{
    private readonly IMediator _mediator;
    public MovimientosCajaController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType<int>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMovimientoCajaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByTurno), new { turnoId = command.TurnoId }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MovimientoCajaDto>>(200)]
    public async Task<IActionResult> GetByTurno([FromQuery] int turnoId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMovimientosByTurnoQuery(turnoId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
