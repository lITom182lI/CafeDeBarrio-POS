using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Common.Helpers;
using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Application.Features.Transacciones.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class ReportesRepository : IReportesRepository
{
    private readonly CafeBarrioDbContext _context;
    public ReportesRepository(CafeBarrioDbContext context) => _context = context;

    public async Task<VentasResumenDto> GetVentasResumenAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var q = _context.Transacciones.Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null);
        var total = await q.SumAsync(t => (decimal?)t.Total, ct) ?? 0;
        var count = await q.CountAsync(ct);
        return new VentasResumenDto(total, count, count > 0 ? MoneyRounding.Round(total / count) : 0, desde, hasta);
    }

    public async Task<IReadOnlyList<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        return await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .GroupBy(t => t.MetodoPago.Nombre)
            .Select(g => new VentasPorMetodoPagoDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopProductoDto>> GetTopProductosAsync(int sedeId, DateTime desde, DateTime hasta, int top, CancellationToken ct)
    {
        return await _context.DetallesTransaccion
            .Where(d => d.Transaccion.SedeId == sedeId && d.Transaccion.Fecha >= desde && d.Transaccion.Fecha <= hasta)
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
            .Select(g => new TopProductoDto(g.Key.ProductoId, g.Key.Nombre, g.Sum(d => d.Cantidad), g.Sum(d => d.SubtotalLinea)))
            .OrderByDescending(x => x.CantidadVendida)
            .Take(top)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VentasPorHoraDto>> GetVentasPorFranjaHorariaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        return await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .GroupBy(t => t.Fecha.Hour)
            .Select(g => new VentasPorHoraDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .OrderBy(g => g.Hora)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AnulacionResumenDto>> GetAnulacionesAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
        => await _context.Anulaciones
            .Where(a => a.Transaccion.SedeId == sedeId && a.FechaHora >= desde && a.FechaHora <= hasta)
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AnulacionResumenDto(a.AnulacionId, a.TransaccionId, a.TipoAnulacion, a.Motivo, a.MontoOriginal, a.MontoDevuelto, a.FechaHora))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StockBajoDto>> GetStockBajoAsync(CancellationToken ct)
        => await _context.Productos
            .Where(p => p.Activo && p.SeguimientoInventario && p.CantidadDisponible <= p.StockMinimo)
            .OrderBy(p => p.CantidadDisponible)
            .Select(p => new StockBajoDto(p.ProductoId, p.Nombre, p.CantidadDisponible, p.StockMinimo, p.UnidadMedida))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TransaccionListItemDto>> GetTransaccionesListAsync(int sedeId, CancellationToken ct)
        => await _context.Transacciones
            .Where(t => t.SedeId == sedeId)
            .OrderByDescending(t => t.Fecha)
            .Take(200)
            .Select(t => new TransaccionListItemDto(
                t.TransaccionId,
                t.RazonSocial != null ? t.RazonSocial : (t.Cliente != null ? t.Cliente.Nombre : "Sin cliente"),
                t.Total,
                t.Fecha,
                t.MetodoPago.Nombre,
                t.Anulacion != null,
                t.Operador != null ? t.Operador.Nombre : null,
                t.TipoDocumento,
                t.NumeroDocumento,
                t.RazonSocial,
                t.MetodoPagoSecundario != null ? t.MetodoPagoSecundario.Nombre : null,
                t.Anulacion != null ? t.Anulacion.Motivo : null))
            .ToListAsync(ct);

    public async Task<TransaccionDetalleDto?> GetTransaccionDetalleAsync(int transaccionId, CancellationToken ct)
    {
        var t = await _context.Transacciones
            .Include(t => t.Cliente)
            .Include(t => t.MetodoPago)
            .Include(t => t.Anulacion)
            .Include(t => t.Operador)
            .Include(t => t.MetodoPagoSecundario)
            .Include(t => t.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(t => t.TransaccionId == transaccionId, ct);

        if (t is null) return null;

        return new TransaccionDetalleDto(
            t.TransaccionId,
            t.RazonSocial != null ? t.RazonSocial : (t.Cliente != null ? t.Cliente.Nombre : "Sin cliente"),
            t.Total,
            t.Subtotal,
            t.Impuesto,
            t.Fecha,
            t.MetodoPago.Nombre,
            t.Anulacion != null,
            t.Detalles.Select(d => new DetalleItemDto(
                d.Producto.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                d.SubtotalLinea)).ToList(),
            t.Operador?.Nombre,
            t.TipoDocumento,
            t.NumeroDocumento,
            t.RazonSocial,
            t.MetodoPagoSecundario?.Nombre,
            t.MontoMetodoPrimario,
            t.Anulacion?.Motivo);
    }

    public async Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var rows = await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .Select(t => new { t.Fecha, t.Total })
            .ToListAsync(ct);

        return rows
            .GroupBy(t => t.Fecha.Date)
            .Select(g => new VentasPorDiaDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .OrderBy(x => x.Fecha)
            .ToList();
    }

    public async Task<IReadOnlyList<TurnoCerradoDto>> GetTurnosCerradosAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var turnos = await _context.Turnos
            .Include(t => t.Operador)
            .Where(t => t.SedeId == sedeId && t.FechaCierre != null && t.FechaApertura >= desde && t.FechaApertura <= hasta)
            .OrderByDescending(t => t.FechaApertura)
            .ToListAsync(ct);

        return turnos.Select(t => {
            var diferencia = (t.MontoEfectivoCierto ?? 0) - (t.TotalEfectivoSistema ?? 0);
            var estado = diferencia < 0 ? "Faltante" : (diferencia > 0 ? "Sobrante" : "Cuadrado");

            return new TurnoCerradoDto(
                t.TurnoId,
                t.OperadorId,
                t.Operador.Nombre,
                t.FechaApertura,
                t.FechaCierre!.Value,
                t.MontoApertura,
                t.MontoEfectivoCierto ?? 0,
                t.TotalEfectivoSistema ?? 0,
                diferencia,
                estado,
                t.Observaciones
            );
        }).ToList();
    }
}
