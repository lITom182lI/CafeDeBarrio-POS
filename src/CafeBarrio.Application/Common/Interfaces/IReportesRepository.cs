using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Application.Features.Transacciones.Dtos;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IReportesRepository
{
    Task<VentasResumenDto> GetVentasResumenAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
    Task<IReadOnlyList<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
    Task<IReadOnlyList<TopProductoDto>> GetTopProductosAsync(int sedeId, DateTime desde, DateTime hasta, int top, CancellationToken ct);
    Task<IReadOnlyList<VentasPorHoraDto>> GetVentasPorFranjaHorariaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
    Task<IReadOnlyList<AnulacionResumenDto>> GetAnulacionesAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
    Task<IReadOnlyList<StockBajoDto>> GetStockBajoAsync(CancellationToken ct);
    Task<IReadOnlyList<TransaccionListItemDto>> GetTransaccionesListAsync(int sedeId, CancellationToken ct);
    Task<TransaccionDetalleDto?> GetTransaccionDetalleAsync(int transaccionId, CancellationToken ct);
    Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
    Task<IReadOnlyList<TurnoCerradoDto>> GetTurnosCerradosAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct);
}
