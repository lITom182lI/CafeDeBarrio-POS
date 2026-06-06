using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetTopProductos;

public class GetTopProductosHandler : IRequestHandler<GetTopProductosQuery, Result<IReadOnlyList<TopProductoDto>>>
{
    private readonly IReportesRepository _repo;
    public GetTopProductosHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<TopProductoDto>>> Handle(GetTopProductosQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dtos = await _repo.GetTopProductosAsync(request.SedeId, desde, hasta, request.Top, ct);
        return Result<IReadOnlyList<TopProductoDto>>.Success(dtos);
    }
}
