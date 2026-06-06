using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasResumen;

public class GetVentasResumenHandler : IRequestHandler<GetVentasResumenQuery, Result<VentasResumenDto>>
{
    private readonly IReportesRepository _repo;
    public GetVentasResumenHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<VentasResumenDto>> Handle(GetVentasResumenQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dto = await _repo.GetVentasResumenAsync(request.SedeId, desde, hasta, ct);
        return Result<VentasResumenDto>.Success(dto);
    }
}
