namespace CafeBarrio.POS.Data.Models;

public class PendingTransaccion
{
    public int Id { get; set; }
    public int SedeId { get; set; }
    public int MetodoPagoId { get; set; }
    public int? OperadorId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaLocal { get; set; }
    public string ItemsJson { get; set; } = string.Empty;
    public bool Sincronizada { get; set; }
    public DateTime? FechaSincronizacion { get; set; }
    public int? TransaccionIdServidor { get; set; }
    public string? ErrorSincronizacion { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? RazonSocial { get; set; }
}
