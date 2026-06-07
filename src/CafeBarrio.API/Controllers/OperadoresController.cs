using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;
using CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.API.Controllers;

public record ValidarPinRequest(int OperadorId, string Pin);
public record OperadorLoginDto(int OperadorId, string Nombre);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OperadoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CafeBarrioDbContext _db;
    private readonly IPasswordHasher _hasher;

    public OperadoresController(IMediator mediator, CafeBarrioDbContext db, IPasswordHasher hasher)
    {
        _mediator = mediator;
        _db       = db;
        _hasher   = hasher;
    }

    [HttpGet]
    [AllowAnonymous]
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

    [HttpPost("validar-pin")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarPin(
        [FromBody] ValidarPinRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest("PIN requerido.");

        var operador = await _db.Set<Operador>()
            .FirstOrDefaultAsync(o => o.OperadorId == request.OperadorId && o.Activo, ct);

        if (operador is null)
            return NotFound("Operador no encontrado.");

        return _hasher.Verify(request.Pin, operador.PinHash)
            ? Ok(new OperadorLoginDto(operador.OperadorId, operador.Nombre))
            : Unauthorized("PIN incorrecto.");
    }
}
