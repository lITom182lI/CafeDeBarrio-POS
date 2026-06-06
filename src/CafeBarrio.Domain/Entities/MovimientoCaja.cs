namespace CafeBarrio.Domain.Entities;

public class MovimientoCaja
{
    public int MovimientoCajaId { get; set; }
    public int TurnoId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }

    public Turno Turno { get; set; } = null!;
}
