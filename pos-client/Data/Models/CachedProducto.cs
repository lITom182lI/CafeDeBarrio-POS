namespace CafeBarrio.POS.Data.Models;

public class CachedProducto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal CantidadDisponible { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CachedAt { get; set; }
}
