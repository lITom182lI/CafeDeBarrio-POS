namespace CafeBarrio.Application.Features.Anulaciones.Dtos;

public record AnulacionDto(
    int AnulacionId,
    int TransaccionId,
    string TipoAnulacion,
    string Motivo,
    decimal MontoOriginal,
    decimal MontoDevuelto,
    string? MetodoDevolucion,
    int OperadorSolicitanteId,
    string OperadorSolicitanteNombre,
    int AutorizadorId,
    string AutorizadorNombre,
    DateTime FechaHora,
    bool ImpactoInventario
);
