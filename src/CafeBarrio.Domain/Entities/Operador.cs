namespace CafeBarrio.Domain.Entities;

public class Operador
{
    public int OperadorId { get; set; }
    public int SedeId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public Sede Sede { get; set; } = null!;
    public ICollection<Turno> Turnos { get; set; } = [];
    public ICollection<Transaccion> Transacciones { get; set; } = [];
    public ICollection<Anulacion> AnulacionesSolicitadas { get; set; } = [];
    public ICollection<Anulacion> AnulacionesAutorizadas { get; set; } = [];
}
