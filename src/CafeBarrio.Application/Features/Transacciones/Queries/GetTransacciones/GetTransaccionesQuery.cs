using CafeBarrio.Application.Features.Transacciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Queries.GetTransacciones;
public record GetTransaccionesQuery(int SedeId) : IRequest<Result<IReadOnlyList<TransaccionListItemDto>>>;
