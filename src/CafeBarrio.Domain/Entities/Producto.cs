using CafeBarrio.Domain.Common;

namespace CafeBarrio.Domain.Entities;

public class Producto : IAuditable
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal StockMinimo { get; set; }
    public string UnidadMedida { get; set; } = "unidad";
    public bool SeguimientoInventario { get; set; } = true;
    public string? CantidadPorUnidad { get; set; }
    public string? ImagenUrl { get; set; }
    public bool EsMayorista { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public CategoriaCafe Categoria { get; set; } = null!;
    public ICollection<DetalleTransaccion> Detalles { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
