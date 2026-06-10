using MediatR;
using MUIS_CORE.Wrappers;
using CafeBarrio.Application.Features.Auth.Dtos;

namespace CafeBarrio.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
