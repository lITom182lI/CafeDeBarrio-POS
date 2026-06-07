namespace CafeBarrio.POS.Dtos;
public record CreateTransaccionRequest(
    int SedeId,
    int? ClienteId,
    int MetodoPagoId,
    List<ItemDto> Items,
    int? OperadorId = null,
    string? TipoDocumento = null,
    string? NumeroDocumento = null,
    string? RazonSocial = null);
