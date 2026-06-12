using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class Transaccion : IAuditable, IAggregateRoot
{
    public int TransaccionId { get; set; }
    public int? ClienteId { get; set; }
    public int SedeId { get; set; }
    public int MetodoPagoId { get; set; }
    public int? MetodoPagoSecundarioId { get; set; }
    public decimal? MontoMetodoPrimario { get; set; }
    public int? OpcionEnvioId { get; set; }
    public int? TurnoId { get; set; }
    public int? OperadorId { get; set; }
    public bool EsMayorista { get; set; }
    public string Canal { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal RecargoPropina { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }
    public string? Notas { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? RazonSocial { get; set; }


    public Cliente Cliente { get; set; } = null!;
    public Sede Sede { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public MetodoPago? MetodoPagoSecundario { get; set; }
    public OpcionEnvio? OpcionEnvio { get; set; }
    public Turno? Turno { get; set; }
    public Operador? Operador { get; set; }
    public ICollection<DetalleTransaccion> Detalles { get; set; } = [];
    public TransaccionMayorista? TransaccionMayorista { get; set; }
    public Anulacion? Anulacion { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public string SunatEstado { get; set; } = "Pendiente";
    public string? SunatError  { get; set; }
    public string? SunatNumeroSerie { get; set; }
    public int SunatIntentos { get; set; }
}
