using CafeBarrio.Application.Features.Sunat.Commands.ReprocesarSunat;
using CafeBarrio.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/sunat")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class SunatController : ControllerBase
{
    private readonly IMediator _mediator;
    public SunatController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{transaccionId:int}/reprocesar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reprocesar(int transaccionId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ReprocesarSunatCommand(transaccionId), ct);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}
