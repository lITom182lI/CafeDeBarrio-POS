using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Configuracion.Queries.GetTasas;

public class GetTasasHandler : IRequestHandler<GetTasasQuery, Result<TasasDto>>
{
    private readonly IConfiguracionNegocioRepository _repo;
    public GetTasasHandler(IConfiguracionNegocioRepository repo) => _repo = repo;

    public async Task<Result<TasasDto>> Handle(GetTasasQuery request, CancellationToken ct)
    {
        var config = await _repo.GetActivaBySedeAsync(request.SedeId, ct);
        var tasa = config is not null ? config.TasaIGV + config.TasaIPM : 0.18m;
        return Result<TasasDto>.Success(new TasasDto(tasa));
    }
}
