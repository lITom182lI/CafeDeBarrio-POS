using CafeBarrio.Application.Features.Productos.Commands.CreateProducto;
using Microsoft.AspNetCore.RateLimiting;
using CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;
using CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUIS_CORE.Pagination;

using CafeBarrio.Domain.Common;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType<PagedResult<ProductoDto>>(200)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 20,
        CancellationToken ct       = default)
    {
        var result = await _mediator.Send(new GetProductosPagedQuery(pageNumber, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [EnableRateLimiting("api-write-policy")]
    [ProducesResponseType<int>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPaged), new { }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductoCommand command, CancellationToken ct)
    {
        if (id != command.ProductoId) return BadRequest("ID no coincide.");
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CafeBarrio.Application.Features.Productos.Commands.DeleteProducto.DeleteProductoCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }
}
