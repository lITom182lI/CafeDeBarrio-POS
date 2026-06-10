namespace CafeBarrio.Infrastructure.External.Sunat;

public sealed class SunatOptions
{
    public const string Section = "Sunat";

    public bool    Enabled         { get; init; }
    public string  Ruc             { get; init; } = "";
    public string  RazonSocial     { get; init; } = "";
    public string  NombreComercial { get; init; } = "";
    public string  DireccionFiscal { get; init; } = "";
    public string  OseApiUrl       { get; init; } = "https://api.nubefact.com/api/v1";
    public string  OseToken        { get; init; } = "";
    public string  SerieBoletaBx   { get; init; } = "B001";
    public decimal PorcentajeIgv   { get; init; } = 18.0m;
}
