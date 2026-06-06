using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Application.Features.Transacciones.Queries.GetTransacciones;
using CafeBarrio.Application.Features.Transacciones.Queries.GetTransaccionDetalle;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransaccionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransaccionCommand command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Create), new { id = result.Value }, null)
            : BadRequest(result.Errors);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLista([FromQuery] int sedeId)
    {
        var result = await _mediator.Send(new GetTransaccionesQuery(sedeId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var result = await _mediator.Send(new GetTransaccionDetalleQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
    }
}
