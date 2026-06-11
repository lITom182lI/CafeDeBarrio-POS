namespace CafeBarrio.Application.Features.Turnos.Dtos;

public sealed record ResumenEfectivoDto(
    decimal MontoApertura,
    decimal TotalVentasEfectivo,
    decimal TotalAnulacionesEfectivo,
    decimal TotalEntradasCaja,
    decimal TotalSalidasCaja)
{
    public decimal SaldoEsperado =>
        MontoApertura
        + TotalVentasEfectivo
        - TotalAnulacionesEfectivo
        + TotalEntradasCaja
        - TotalSalidasCaja;
}
