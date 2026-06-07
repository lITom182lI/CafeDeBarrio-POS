using CafeBarrio.Application.Features.Catalogos.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Catalogos.Queries.GetCategorias;

public record GetCategoriasQuery : IRequest<Result<IReadOnlyList<CategoriaCafeDto>>>;
