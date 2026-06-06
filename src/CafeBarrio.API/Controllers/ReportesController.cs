using CafeBarrio.Application.Features.Reportes.Queries.GetVentasResumen;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorMetodoPago;
using CafeBarrio.Application.Features.Reportes.Queries.GetTopProductos;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorFranjaHoraria;
using CafeBarrio.Application.Features.Reportes.Queries.GetAnulaciones;
using CafeBarrio.Application.Features.Reportes.Queries.GetStockBajo;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorDia;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("ventas-resumen")]
    public async Task<IActionResult> GetVentasResumen([FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        var result = await _mediator.Send(new GetVentasResumenQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-metodo-pago")]
    public async Task<IActionResult> GetVentasPorMetodoPago([FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        var result = await _mediator.Send(new GetVentasPorMetodoPagoQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("top-productos")]
    public async Task<IActionResult> GetTopProductos([FromQuery] int sedeId, [FromQuery] string periodo = "dia", [FromQuery] int top = 5)
    {
        var result = await _mediator.Send(new GetTopProductosQuery(sedeId, periodo, top));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-hora")]
    public async Task<IActionResult> GetVentasPorHora([FromQuery] int sedeId, [FromQuery] string periodo = "dia")
    {
        var result = await _mediator.Send(new GetVentasPorFranjaHorariaQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("anulaciones")]
    public async Task<IActionResult> GetAnulaciones([FromQuery] int sedeId, [FromQuery] string periodo = "mes")
    {
        var result = await _mediator.Send(new GetAnulacionesQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("stock-bajo")]
    public async Task<IActionResult> GetStockBajo()
    {
        var result = await _mediator.Send(new GetStockBajoQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("ventas-por-dia")]
    public async Task<IActionResult> GetVentasPorDia([FromQuery] int sedeId, [FromQuery] string periodo = "semana")
    {
        var result = await _mediator.Send(new GetVentasPorDiaQuery(sedeId, periodo));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
