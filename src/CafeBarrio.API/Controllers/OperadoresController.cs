using CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;
using CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OperadoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CafeBarrioDbContext _db;

    public OperadoresController(IMediator mediator, CafeBarrioDbContext db)
    {
        _mediator = mediator;
        _db       = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await _db.Set<Operador>()
            .OrderBy(o => o.Nombre)
            .Select(o => new { o.OperadorId, o.Nombre, o.Activo })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOperadorCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateOperadorCommand command, CancellationToken ct)
    {
        if (id != command.OperadorId) return BadRequest("ID no coincide.");
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }
}
