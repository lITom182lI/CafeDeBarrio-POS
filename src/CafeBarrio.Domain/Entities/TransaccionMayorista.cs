namespace CafeBarrio.Domain.Entities;

public class TransaccionMayorista
{
    public int TmId { get; set; }
    public int TransaccionId { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public int? TransporteId { get; set; }
    public string? InstruccionesEntrega { get; set; }
    public DateOnly? FechaEntregaEstimada { get; set; }

    public Transaccion Transaccion { get; set; } = null!;
    public Transporte? Transporte { get; set; }
}
