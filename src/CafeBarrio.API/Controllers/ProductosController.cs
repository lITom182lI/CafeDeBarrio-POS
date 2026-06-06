using CafeBarrio.Application.Features.Productos.Commands.CreateProducto;
using CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;
using CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProductosPagedQuery(pageNumber, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPaged), new { }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductoCommand command, CancellationToken ct)
    {
        if (id != command.ProductoId) return BadRequest("ID no coincide.");
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }
}
