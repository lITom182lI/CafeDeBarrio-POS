using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Anulaciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Anulaciones.Queries.GetAnulaciones;

public class GetAnulacionesHandler : IRequestHandler<GetAnulacionesQuery, Result<IReadOnlyList<AnulacionDto>>>
{
    private readonly IAnulacionRepository _anulaciones;

    public GetAnulacionesHandler(IAnulacionRepository anulaciones) => _anulaciones = anulaciones;

    public async Task<Result<IReadOnlyList<AnulacionDto>>> Handle(GetAnulacionesQuery request, CancellationToken ct)
    {
        var items = await _anulaciones.GetBySedeAsync(request.SedeId, ct);
        var dtos = items.Select(a => new AnulacionDto(
            a.AnulacionId,
            a.TransaccionId,
            a.TipoAnulacion,
            a.Motivo,
            a.MontoOriginal,
            a.MontoDevuelto,
            a.MetodoDevolucion,
            a.OperadorSolicitanteId,
            a.OperadorSolicitante?.Nombre ?? string.Empty,
            a.AutorizadorId,
            a.Autorizador?.Nombre ?? string.Empty,
            a.FechaHora,
            a.ImpactoInventario
        )).ToList();

        return Result<IReadOnlyList<AnulacionDto>>.Success(dtos);
    }
}
