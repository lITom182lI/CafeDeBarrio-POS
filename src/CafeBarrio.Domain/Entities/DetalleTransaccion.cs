namespace CafeBarrio.Domain.Entities;

public class DetalleTransaccion
{
    public int DetalleId { get; set; }
    public int TransaccionId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }

    public Transaccion Transaccion { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
