using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class DetalleTransaccion : IAuditable
{
    public int DetalleId { get; set; }
    public int TransaccionId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }

    public Transaccion Transaccion { get; set; } = null!;
    public Producto Producto { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
