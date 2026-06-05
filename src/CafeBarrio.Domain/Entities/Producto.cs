namespace CafeBarrio.Domain.Entities;

public class Producto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public int CantidadDisponible { get; set; }
    public string? CantidadPorUnidad { get; set; }
    public string? ImagenUrl { get; set; }
    public bool EsMayorista { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public CategoriaCafe Categoria { get; set; } = null!;
    public ICollection<DetalleTransaccion> Detalles { get; set; } = [];
}
