using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class Turno : IAuditable
{
    public int TurnoId { get; set; }
    public int SedeId { get; set; }
    public int OperadorId { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal? MontoEfectivoCierto { get; set; }
    public decimal? TotalEfectivoSistema { get; set; }
    public string Estado { get; set; } = "Abierto";
    public string? Observaciones { get; set; }

    public Sede Sede { get; set; } = null!;
    public Operador Operador { get; set; } = null!;
    public ICollection<Transaccion> Transacciones { get; set; } = [];
    public ICollection<MovimientoCaja> Movimientos { get; set; } = [];

    // Audit
    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy   { get; set; }
    public string? UpdatedBy   { get; set; }
}
