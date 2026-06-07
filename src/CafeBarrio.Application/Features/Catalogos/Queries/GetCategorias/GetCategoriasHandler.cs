using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Catalogos.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Catalogos.Queries.GetCategorias;

public class GetCategoriasHandler
    : IRequestHandler<GetCategoriasQuery, Result<IReadOnlyList<CategoriaCafeDto>>>
{
    private readonly ICategoriaCafeRepository _categorias;

    public GetCategoriasHandler(ICategoriaCafeRepository categorias)
        => _categorias = categorias;

    public async Task<Result<IReadOnlyList<CategoriaCafeDto>>> Handle(
        GetCategoriasQuery request, CancellationToken ct)
    {
        var entities = await _categorias.GetAllActivasAsync(ct);
        IReadOnlyList<CategoriaCafeDto> dtos = entities
            .Select(c => new CategoriaCafeDto(c.CategoriaId, c.Codigo, c.Nombre))
            .OrderBy(c => c.Nombre)
            .ToList();
        return Result<IReadOnlyList<CategoriaCafeDto>>.Success(dtos);
    }
}
