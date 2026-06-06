using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Transacciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Queries.GetTransaccionDetalle;

public class GetTransaccionDetalleHandler : IRequestHandler<GetTransaccionDetalleQuery, Result<TransaccionDetalleDto>>
{
    private readonly IReportesRepository _repo;
    public GetTransaccionDetalleHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<TransaccionDetalleDto>> Handle(GetTransaccionDetalleQuery request, CancellationToken ct)
    {
        var detalle = await _repo.GetTransaccionDetalleAsync(request.TransaccionId, ct);
        return detalle is null
            ? Result<TransaccionDetalleDto>.Failure(new Error("Transaccion.NotFound", "Transacción no encontrada."))
            : Result<TransaccionDetalleDto>.Success(detalle);
    }
}
