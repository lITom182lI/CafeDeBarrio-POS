namespace CafeBarrio.Domain.Entities;

public class Transporte
{
    public int TransporteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal? CapacidadKg { get; set; }
    public bool Disponible { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<TransaccionMayorista> TransaccionesMayoristas { get; set; } = [];
}
