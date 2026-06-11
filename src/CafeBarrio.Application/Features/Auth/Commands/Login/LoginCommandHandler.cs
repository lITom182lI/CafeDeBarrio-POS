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
    private readonly IUnitOfWork _uow;

    public LoginCommandHandler(IUsuarioRepository usuarios, IJwtService jwt, IPasswordHasher hasher, IUnitOfWork uow)
    {
        _usuarios = usuarios;
        _jwt = jwt;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null)
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Credenciales incorrectas"));

        if (usuario.IsLocked && usuario.LockedUntil > DateTime.UtcNow)
            return Result<LoginResponse>.Failure(new Error("Cuenta.Bloqueada", "Cuenta bloqueada temporalmente. Intente nuevamente más tarde."));

        if (usuario.IsLocked && usuario.LockedUntil <= DateTime.UtcNow)
        {
            usuario.IsLocked = false;
            usuario.FailedLoginAttempts = 0;
            usuario.LockedUntil = null;
        }

        if (!_hasher.Verify(request.Password, usuario.PasswordHash))
        {
            usuario.FailedLoginAttempts++;

            if (usuario.FailedLoginAttempts >= 5)
            {
                usuario.IsLocked = true;
                usuario.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }

            await _uow.SaveChangesAsync(ct);
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Credenciales incorrectas"));
        }

        usuario.FailedLoginAttempts = 0;
        usuario.IsLocked = false;
        usuario.LockedUntil = null;
        
        await _uow.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(usuario);
        return Result<LoginResponse>.Success(new LoginResponse(token, usuario.Rol, usuario.Email));
    }
}
