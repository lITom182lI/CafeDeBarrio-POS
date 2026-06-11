using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using Microsoft.AspNetCore.RateLimiting;
using CafeBarrio.Application.Features.Transacciones.Dtos;
using CafeBarrio.Application.Features.Transacciones.Queries.GetTransacciones;
using CafeBarrio.Application.Features.Transacciones.Queries.GetTransaccionDetalle;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TransaccionesController : ControllerBase
{
    private readonly IMediator _mediator;
    public TransaccionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [EnableRateLimiting("api-write-policy")]
    [ProducesResponseType<int>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransaccionCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TransaccionListItemDto>>(200)]
    public async Task<IActionResult> GetLista([FromQuery] int sedeId)
    {
        var result = await _mediator.Send(new GetTransaccionesQuery(sedeId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<TransaccionDetalleDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var result = await _mediator.Send(new GetTransaccionDetalleQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpPost("{id}/anular")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Anular(int id, [FromBody] CafeBarrio.Application.Features.Transacciones.Commands.AnularTransaccion.AnularTransaccionCommand command, CancellationToken ct = default)
    {
        if (id != command.TransaccionId)
            return BadRequest(new { Error = "El ID de la ruta no coincide con el comando." });

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}
