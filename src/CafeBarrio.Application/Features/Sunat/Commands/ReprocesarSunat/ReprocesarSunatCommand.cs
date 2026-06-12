using MUIS_CORE.Wrappers;
using MediatR;

namespace CafeBarrio.Application.Features.Sunat.Commands.ReprocesarSunat;

public record ReprocesarSunatCommand(int TransaccionId) : IRequest<Result>;
