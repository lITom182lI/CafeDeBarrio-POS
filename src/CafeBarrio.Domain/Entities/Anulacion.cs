using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class Anulacion : IAuditable, IAggregateRoot
{
    public int AnulacionId { get; set; }
    public int TransaccionId { get; set; }
    public string TipoAnulacion { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal MontoDevuelto { get; set; }
    public string? MetodoDevolucion { get; set; }
    public int OperadorSolicitanteId { get; set; }
    public int AutorizadorId { get; set; }
    public DateTime FechaHora { get; set; }
    public bool ImpactoInventario { get; set; } = true;

    public Transaccion Transaccion { get; set; } = null!;
    public Operador OperadorSolicitante { get; set; } = null!;
    public Operador Autorizador { get; set; } = null!;

    // Audit
    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy   { get; set; }
    public string? UpdatedBy   { get; set; }
}
