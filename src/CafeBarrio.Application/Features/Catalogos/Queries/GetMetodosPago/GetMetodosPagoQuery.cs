using CafeBarrio.Application.Features.Catalogos.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Catalogos.Queries.GetMetodosPago;

public record GetMetodosPagoQuery : IRequest<Result<IReadOnlyList<MetodoPagoDto>>>;
