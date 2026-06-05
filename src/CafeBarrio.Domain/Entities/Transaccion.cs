namespace CafeBarrio.Domain.Entities;

public class Transaccion
{
    public int TransaccionId { get; set; }
    public int ClienteId { get; set; }
    public int SedeId { get; set; }
    public int MetodoPagoId { get; set; }
    public int? OpcionEnvioId { get; set; }
    public bool EsMayorista { get; set; }
    public string Canal { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }
    public string? Notas { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Sede Sede { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public OpcionEnvio? OpcionEnvio { get; set; }
    public ICollection<DetalleTransaccion> Detalles { get; set; } = [];
    public TransaccionMayorista? TransaccionMayorista { get; set; }
}
