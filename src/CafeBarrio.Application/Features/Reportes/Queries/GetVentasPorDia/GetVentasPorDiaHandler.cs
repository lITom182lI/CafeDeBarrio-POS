using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorDia;

public class GetVentasPorDiaHandler : IRequestHandler<GetVentasPorDiaQuery, Result<IReadOnlyList<VentasPorDiaDto>>>
{
    private readonly IReportesRepository _repo;

    public GetVentasPorDiaHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VentasPorDiaDto>>> Handle(GetVentasPorDiaQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var result = await _repo.GetVentasPorDiaAsync(request.SedeId, desde, hasta, ct);
        return Result<IReadOnlyList<VentasPorDiaDto>>.Success(result);
    }
}
