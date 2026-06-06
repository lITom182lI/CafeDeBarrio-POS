using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetAnulaciones;

public class GetAnulacionesHandler : IRequestHandler<GetAnulacionesQuery, Result<IReadOnlyList<AnulacionResumenDto>>>
{
    private readonly IReportesRepository _repo;
    public GetAnulacionesHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<AnulacionResumenDto>>> Handle(GetAnulacionesQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dtos = await _repo.GetAnulacionesAsync(request.SedeId, desde, hasta, ct);
        return Result<IReadOnlyList<AnulacionResumenDto>>.Success(dtos);
    }
}
