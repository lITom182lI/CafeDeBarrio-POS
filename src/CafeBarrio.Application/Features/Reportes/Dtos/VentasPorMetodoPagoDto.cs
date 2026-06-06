namespace CafeBarrio.Application.Features.Reportes.Dtos;
public record VentasPorMetodoPagoDto(string MetodoPago, decimal TotalVentas, int NumTransacciones);
