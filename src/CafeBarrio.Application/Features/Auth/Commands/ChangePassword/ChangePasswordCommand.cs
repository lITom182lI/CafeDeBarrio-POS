using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string Email, string CurrentPassword, string NewPassword) : IRequest<Result>;
