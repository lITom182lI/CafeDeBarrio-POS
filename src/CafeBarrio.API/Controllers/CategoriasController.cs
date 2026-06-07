using CafeBarrio.Application.Features.Catalogos.Dtos;
using CafeBarrio.Application.Features.Catalogos.Queries.GetCategorias;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ISender _sender;
    public CategoriasController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoriaCafeDto>>(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCategoriasQuery(), ct);
        return Ok(result.Value);
    }
}
