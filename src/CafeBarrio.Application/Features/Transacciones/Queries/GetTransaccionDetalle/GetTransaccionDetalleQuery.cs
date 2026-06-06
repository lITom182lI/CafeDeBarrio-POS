using CafeBarrio.Application.Features.Transacciones.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Queries.GetTransaccionDetalle;
public record GetTransaccionDetalleQuery(int TransaccionId) : IRequest<Result<TransaccionDetalleDto>>;
