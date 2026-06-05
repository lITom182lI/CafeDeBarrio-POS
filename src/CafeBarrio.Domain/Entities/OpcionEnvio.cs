namespace CafeBarrio.Domain.Entities;

public class OpcionEnvio
{
    public int OpcionEnvioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Tarifa { get; set; }
    public bool Activa { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = [];
}
