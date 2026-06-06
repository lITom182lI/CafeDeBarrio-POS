namespace CafeBarrio.Application.Features.Reportes.Dtos;
public record VentasResumenDto(decimal TotalVentas, int NumTransacciones, decimal TicketPromedio, DateTime Desde, DateTime Hasta);
