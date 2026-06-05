namespace CafeBarrio.Domain.Entities;

public class CategoriaCafe
{
    public int CategoriaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; }

    public ICollection<Producto> Productos { get; set; } = [];
    public ICollection<Cliente> ClientesConPreferencia { get; set; } = [];
}
