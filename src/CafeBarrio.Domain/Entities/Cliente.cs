namespace CafeBarrio.Domain.Entities;

public class Cliente
{
    public int ClienteId { get; set; }
    public int TipoClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CodigoCliente { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Distrito { get; set; }
    public string? Ciudad { get; set; }
    public int? UbicacionId { get; set; }
    public int? CategoriaPrefId { get; set; }
    public DateOnly FechaRegistro { get; set; }
    public bool Activo { get; set; }
    public string? Observaciones { get; set; }

    public TipoCliente TipoCliente { get; set; } = null!;
    public UbicacionPreferencia? Ubicacion { get; set; }
    public CategoriaCafe? CategoriaPreferida { get; set; }
    public ICollection<Transaccion> Transacciones { get; set; } = [];
}
