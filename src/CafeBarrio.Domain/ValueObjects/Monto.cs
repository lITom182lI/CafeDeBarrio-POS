namespace CafeBarrio.Domain.ValueObjects;

public record Monto
{
    public decimal Valor { get; }
    public static Monto Zero => new(0m);

    public Monto(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("El monto no puede ser negativo.", nameof(valor));
        Valor = valor;
    }

    public static Monto From(decimal valor) => new(valor);
    public static implicit operator decimal(Monto m) => m.Valor;
}
