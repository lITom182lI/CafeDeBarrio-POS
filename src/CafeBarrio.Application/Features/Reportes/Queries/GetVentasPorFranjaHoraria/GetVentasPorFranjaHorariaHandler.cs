using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorFranjaHoraria;

public class GetVentasPorFranjaHorariaHandler : IRequestHandler<GetVentasPorFranjaHorariaQuery, Result<IReadOnlyList<VentasPorHoraDto>>>
{
    private readonly IReportesRepository _repo;
    public GetVentasPorFranjaHorariaHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VentasPorHoraDto>>> Handle(GetVentasPorFranjaHorariaQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dtos = await _repo.GetVentasPorFranjaHorariaAsync(request.SedeId, desde, hasta, ct);
        return Result<IReadOnlyList<VentasPorHoraDto>>.Success(dtos);
    }
}
