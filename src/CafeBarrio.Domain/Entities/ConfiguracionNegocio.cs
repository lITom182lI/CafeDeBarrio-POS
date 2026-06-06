namespace CafeBarrio.Domain.Entities;

public class ConfiguracionNegocio
{
    public int ConfiguracionNegocioId { get; set; }
    public int SedeId { get; set; }
    public decimal TasaIGV { get; set; }
    public decimal TasaIPM { get; set; }
    public DateTime FechaVigencia { get; set; }
    public bool Activo { get; set; } = true;

    public Sede Sede { get; set; } = null!;
}
