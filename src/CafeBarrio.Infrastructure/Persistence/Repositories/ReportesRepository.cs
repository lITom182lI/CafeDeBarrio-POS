using CafeBarrio.Application.Common.Interfaces;
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
        return new VentasResumenDto(total, count, count > 0 ? Math.Round(total / count, 2) : 0, desde, hasta);
    }

    public async Task<IReadOnlyList<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var transacciones = await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .Select(t => new { Metodo = t.MetodoPago.Nombre, t.Total })
            .ToListAsync(ct);

        return transacciones
            .GroupBy(t => t.Metodo)
            .Select(g => new VentasPorMetodoPagoDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .ToList();
    }

    public async Task<IReadOnlyList<TopProductoDto>> GetTopProductosAsync(int sedeId, DateTime desde, DateTime hasta, int top, CancellationToken ct)
    {
        var detalles = await _context.DetallesTransaccion
            .Where(d => d.Transaccion.SedeId == sedeId && d.Transaccion.Fecha >= desde && d.Transaccion.Fecha <= hasta)
            .Select(d => new { d.ProductoId, d.Producto.Nombre, d.Cantidad, d.SubtotalLinea })
            .ToListAsync(ct);

        return detalles
            .GroupBy(d => new { d.ProductoId, d.Nombre })
            .OrderByDescending(g => g.Sum(d => d.Cantidad))
            .Select(g => new TopProductoDto(g.Key.ProductoId, g.Key.Nombre, g.Sum(d => d.Cantidad), g.Sum(d => d.SubtotalLinea)))
            .Take(top)
            .ToList();
    }

    public async Task<IReadOnlyList<VentasPorHoraDto>> GetVentasPorFranjaHorariaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var transacciones = await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .Select(t => new { t.Fecha.Hour, t.Total })
            .ToListAsync(ct);

        return transacciones
            .GroupBy(t => t.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new VentasPorHoraDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .ToList();
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
                t.MetodoPagoSecundario != null ? t.MetodoPagoSecundario.Nombre : null))
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
            t.MontoMetodoPrimario);
    }

    public async Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var transacciones = await _context.Transacciones
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta && t.Anulacion == null)
            .Select(t => new { t.Fecha, t.Total })
            .ToListAsync(ct);

        return transacciones
            .GroupBy(t => t.Fecha.Date)
            .Select(g => new VentasPorDiaDto(g.Key, g.Sum(t => t.Total), g.Count()))
            .OrderBy(x => x.Fecha)
            .ToList();
    }
}
