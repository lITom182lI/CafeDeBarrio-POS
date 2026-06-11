using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasResumen;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorMetodoPago;
using CafeBarrio.Application.Features.Reportes.Queries.GetTopProductos;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorFranjaHoraria;
using CafeBarrio.Application.Features.Reportes.Queries.GetAnulaciones;
using CafeBarrio.Application.Features.Reportes.Queries.GetStockBajo;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorDia;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CafeBarrio.Domain.Common;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("ventas-resumen")]
    [ProducesResponseType<VentasResumenDto>(200)]
    public async Task<IActionResult> GetVentasResumen(
        [FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetVentasResumenQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-metodo-pago")]
    [ProducesResponseType<IReadOnlyList<VentasPorMetodoPagoDto>>(200)]
    public async Task<IActionResult> GetVentasPorMetodoPago(
        [FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetVentasPorMetodoPagoQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("top-productos")]
    [ProducesResponseType<IReadOnlyList<TopProductoDto>>(200)]
    public async Task<IActionResult> GetTopProductos(
        [FromQuery] int sedeId, [FromQuery] string periodo = "dia", [FromQuery] int top = 5)
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetTopProductosQuery(sedeId, periodo, top));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-hora")]
    [ProducesResponseType<IReadOnlyList<VentasPorHoraDto>>(200)]
    public async Task<IActionResult> GetVentasPorHora(
        [FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetVentasPorFranjaHorariaQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("anulaciones")]
    [ProducesResponseType<IReadOnlyList<AnulacionResumenDto>>(200)]
    public async Task<IActionResult> GetAnulaciones(
        [FromQuery] int sedeId, [FromQuery] string periodo = "mes")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetAnulacionesQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("stock-bajo")]
    [ProducesResponseType<IReadOnlyList<StockBajoDto>>(200)]
    public async Task<IActionResult> GetStockBajo()
    {
        var result = await _mediator.Send(new GetStockBajoQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-dia")]
    [ProducesResponseType<IReadOnlyList<VentasPorDiaDto>>(200)]
    public async Task<IActionResult> GetVentasPorDia(
        [FromQuery] int sedeId, [FromQuery] string periodo = "semana")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new GetVentasPorDiaQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("cierres-caja")]
    [ProducesResponseType<IReadOnlyList<TurnoCerradoDto>>(200)]
    public async Task<IActionResult> GetCierresCaja(
        [FromQuery] int sedeId, [FromQuery] string periodo = "mes")
    {
        if (!PeriodoReporte.EsValido(periodo))
            return BadRequest(new { error = "Periodo inválido. Valores permitidos: dia, semana, mes, turno." });

        var result = await _mediator.Send(new CafeBarrio.Application.Features.Reportes.Queries.GetTurnosCerrados.GetTurnosCerradosQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
