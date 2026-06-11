using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;
using CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;
using CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;
using CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;
using CafeBarrio.Application.Features.Operadores.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using CafeBarrio.Domain.Common;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class OperadoresController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IOperadorRepository _operadores;

    public OperadoresController(ISender sender, IOperadorRepository operadores)
    {
        _sender     = sender;
        _operadores = operadores;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OperadorResumenDto>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        // Si el llamador no está autenticado (POS PWA) solo devolvemos activos.
        // Si es un admin autenticado puede pedir todos con soloActivos=false.
        var esAdmin = User.Identity?.IsAuthenticated == true;
        var filtrar = soloActivos || !esAdmin;

        var lista = filtrar
            ? await _operadores.GetAllActivosAsync(ct)
            : await _operadores.GetAllAsync(ct);

        return Ok(lista.Select(o => new OperadorResumenDto(o.OperadorId, o.Nombre, o.Activo, o.Eliminado)).ToList());
    }

    [HttpPost]
    [ProducesResponseType<int>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOperadorCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateOperadorCommand command, CancellationToken ct)
    {
        if (id != command.OperadorId) return BadRequest("ID no coincide.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteOperadorCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors);
    }

    [HttpPost("validar-pin")]
    [AllowAnonymous]
    [EnableRateLimiting("pin-policy")]
    [ProducesResponseType<OperadorLoginDto>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ValidarPin(
        [FromBody] ValidarPinCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Errors);
    }
}
