using CafeBarrio.Application.Features.Catalogos.Dtos;
using CafeBarrio.Application.Features.Catalogos.Queries.GetMetodosPago;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/metodos-pago")]
[AllowAnonymous]
[Produces("application/json")]
public class MetodosPagoController : ControllerBase
{
    private readonly ISender _sender;
    public MetodosPagoController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MetodoPagoDto>>(200)]
    public async Task<IActionResult> GetMetodosPago(CancellationToken ct)
    {
        var result = await _sender.Send(new GetMetodosPagoQuery(), ct);
        return Ok(result.Value);
    }
}
