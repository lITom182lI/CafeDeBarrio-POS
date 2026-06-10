using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetTurnosCerrados;

public class GetTurnosCerradosHandler : IRequestHandler<GetTurnosCerradosQuery, Result<IReadOnlyList<TurnoCerradoDto>>>
{
    private readonly IReportesRepository _repo;
    public GetTurnosCerradosHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<TurnoCerradoDto>>> Handle(GetTurnosCerradosQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dto = await _repo.GetTurnosCerradosAsync(request.SedeId, desde, hasta, ct);
        return Result<IReadOnlyList<TurnoCerradoDto>>.Success(dto);
    }
}
