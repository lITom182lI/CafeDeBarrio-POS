namespace CafeBarrio.Domain.Entities;

public class Sede
{
    public int SedeId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool EsPrincipal { get; set; }
    public bool Activa { get; set; }
    public DateOnly? FechaApertura { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = [];
}
