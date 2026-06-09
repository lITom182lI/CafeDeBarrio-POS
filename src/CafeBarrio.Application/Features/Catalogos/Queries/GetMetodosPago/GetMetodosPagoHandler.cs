using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Catalogos.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Catalogos.Queries.GetMetodosPago;

public class GetMetodosPagoHandler
    : IRequestHandler<GetMetodosPagoQuery, Result<IReadOnlyList<MetodoPagoDto>>>
{
    private readonly IMetodoPagoRepository _metodos;

    public GetMetodosPagoHandler(IMetodoPagoRepository metodos)
        => _metodos = metodos;

    public async Task<Result<IReadOnlyList<MetodoPagoDto>>> Handle(
        GetMetodosPagoQuery request, CancellationToken ct)
    {
        var entities = await _metodos.GetAllAsync(ct);
        IReadOnlyList<MetodoPagoDto> dtos = entities
            .Select(m => new MetodoPagoDto(m.MetodoPagoId, m.Nombre, m.EsEfectivo))
            .ToList();
        return Result<IReadOnlyList<MetodoPagoDto>>.Success(dtos);
    }
}
