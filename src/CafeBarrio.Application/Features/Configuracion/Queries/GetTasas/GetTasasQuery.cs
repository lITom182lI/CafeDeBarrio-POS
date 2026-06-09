using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Configuracion.Queries.GetTasas;

public record TasasDto(decimal TasaIgv);
public record GetTasasQuery(int SedeId) : IRequest<Result<TasasDto>>;
