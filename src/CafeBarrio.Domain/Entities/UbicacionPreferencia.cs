namespace CafeBarrio.Domain.Entities;

public class UbicacionPreferencia
{
    public int UbicacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Cliente> Clientes { get; set; } = [];
}
