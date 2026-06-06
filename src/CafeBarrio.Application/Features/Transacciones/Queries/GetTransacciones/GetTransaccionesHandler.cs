using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Transacciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Queries.GetTransacciones;

public class GetTransaccionesHandler : IRequestHandler<GetTransaccionesQuery, Result<IReadOnlyList<TransaccionListItemDto>>>
{
    private readonly IReportesRepository _repo;
    public GetTransaccionesHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<TransaccionListItemDto>>> Handle(GetTransaccionesQuery request, CancellationToken ct)
    {
        var lista = await _repo.GetTransaccionesListAsync(request.SedeId, ct);
        return Result<IReadOnlyList<TransaccionListItemDto>>.Success(lista);
    }
}
