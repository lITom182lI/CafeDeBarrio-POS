namespace CafeBarrio.Domain.ValueObjects;

public record Documento
{
    private static readonly HashSet<string> TiposValidos =
        ["DNI", "RUC", "CE", "Pasaporte"];

    public string Tipo   { get; }
    public string Numero { get; }

    public Documento(string tipo, string numero)
    {
        if (!TiposValidos.Contains(tipo))
            throw new ArgumentException($"TipoDocumento inválido: '{tipo}'.", nameof(tipo));
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("NumeroDocumento no puede estar vacío.", nameof(numero));
        Tipo   = tipo;
        Numero = numero.Trim();
    }

    public static Documento? TryCreate(string? tipo, string? numero)
    {
        if (tipo is null && numero is null) return null;
        return new Documento(tipo!, numero!);
    }
}
