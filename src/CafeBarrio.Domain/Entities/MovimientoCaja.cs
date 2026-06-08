using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class MovimientoCaja : IAuditable
{
    public int MovimientoCajaId { get; set; }
    public int TurnoId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }

    public Turno Turno { get; set; } = null!;

    // Audit
    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy   { get; set; }
    public string? UpdatedBy   { get; set; }
}
