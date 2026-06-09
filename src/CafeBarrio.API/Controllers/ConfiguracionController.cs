using CafeBarrio.Application.Features.Configuracion.Queries.GetTasas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/configuracion")]
[AllowAnonymous]
[Produces("application/json")]
public class ConfiguracionController : ControllerBase
{
    private readonly ISender _sender;
    public ConfiguracionController(ISender sender) => _sender = sender;

    [HttpGet("tasas")]
    [ProducesResponseType<TasasDto>(200)]
    public async Task<IActionResult> GetTasas([FromQuery] int sedeId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTasasQuery(sedeId), ct);
        return Ok(result.Value);
    }
}
