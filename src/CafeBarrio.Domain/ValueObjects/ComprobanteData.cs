namespace CafeBarrio.Domain.ValueObjects;

public record ComprobanteData(Documento Documento, string? RazonSocial)
{
    public static ComprobanteData? TryCreate(
        string? tipo, string? numero, string? razonSocial)
    {
        var doc = Documento.TryCreate(tipo, numero);
        return doc is null ? null : new ComprobanteData(doc, razonSocial);
    }
}
