using CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;
using CafeBarrio.Application.Features.Anulaciones.Dtos;
using CafeBarrio.Application.Features.Anulaciones.Queries.GetAnulaciones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnulacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnulacionesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType<int>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnulacionCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AnulacionDto>>(200)]
    public async Task<IActionResult> GetBySede([FromQuery] int sedeId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAnulacionesQuery(sedeId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
