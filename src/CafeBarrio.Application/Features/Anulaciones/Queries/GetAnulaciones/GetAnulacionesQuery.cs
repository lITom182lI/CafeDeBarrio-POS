using CafeBarrio.Application.Features.Anulaciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Anulaciones.Queries.GetAnulaciones;

public record GetAnulacionesQuery(int SedeId) : IRequest<Result<IReadOnlyList<AnulacionDto>>>;
