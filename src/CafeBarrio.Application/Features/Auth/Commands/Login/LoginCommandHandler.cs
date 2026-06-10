using MediatR;
using MUIS_CORE.Wrappers;
using CafeBarrio.Application.Features.Auth.Dtos;
using CafeBarrio.Application.Common.Interfaces;

namespace CafeBarrio.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _hasher;

    public LoginCommandHandler(IUsuarioRepository usuarios, IJwtService jwt, IPasswordHasher hasher)
    {
        _usuarios = usuarios;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null || !_hasher.Verify(request.Password, usuario.PasswordHash))
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Credenciales incorrectas"));

        var token = _jwt.GenerateToken(usuario);
        return Result<LoginResponse>.Success(new LoginResponse(token, usuario.Rol, usuario.Email));
    }
}
