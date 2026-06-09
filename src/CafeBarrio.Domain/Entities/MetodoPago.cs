namespace CafeBarrio.Domain.Entities;

public class MetodoPago
{
    public int MetodoPagoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool EsEfectivo { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = [];
}
