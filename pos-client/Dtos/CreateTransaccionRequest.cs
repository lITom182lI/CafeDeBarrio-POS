namespace CafeBarrio.POS.Dtos;
public record CreateTransaccionRequest(
    int SedeId,
    int? ClienteId,
    int MetodoPagoId,
    List<ItemDto> Items);
