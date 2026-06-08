using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;

    public ChangePasswordHandler(IUsuarioRepository usuarios, IPasswordHasher hasher, IUnitOfWork uow)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null)
            return new Error("Auth.UserNotFound", "Usuario no encontrado.");

        if (!_hasher.Verify(request.CurrentPassword, usuario.PasswordHash))
            return new Error("Auth.InvalidCurrentPassword", "La contraseña actual es incorrecta.");

        usuario.PasswordHash = _hasher.Hash(request.NewPassword);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}
