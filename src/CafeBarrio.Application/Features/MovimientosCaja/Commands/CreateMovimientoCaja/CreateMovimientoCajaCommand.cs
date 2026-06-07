using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;

public record CreateMovimientoCajaCommand(
    int TurnoId,
    string TipoMovimiento,
    string Motivo,
    decimal Monto
) : IRequest<Result<int>>;
