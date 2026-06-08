using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Auth.Commands.ChangePassword;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Auth;

public class ChangePasswordHandlerTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher      = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _uow             = Substitute.For<IUnitOfWork>();
    private readonly ChangePasswordHandler _sut;

    public ChangePasswordHandlerTests() =>
        _sut = new ChangePasswordHandler(_usuarios, _hasher, _uow);

    [Fact]
    public async Task Handle_UsuarioNotFound_ReturnsFailure()
    {
        _usuarios.GetByEmailAsync("no@existe.com", Arg.Any<CancellationToken>())
                 .Returns((Usuario?)null);

        var result = await _sut.Handle(
            new ChangePasswordCommand("no@existe.com", "old", "newpass1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_PasswordActualIncorrecto_ReturnsFailure()
    {
        _usuarios.GetByEmailAsync("u@x.com", Arg.Any<CancellationToken>())
                 .Returns(new Usuario { Email = "u@x.com", PasswordHash = "hash" });
        _hasher.Verify("wrongpass", "hash").Returns(false);

        var result = await _sut.Handle(
            new ChangePasswordCommand("u@x.com", "wrongpass", "newpass1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Auth.InvalidCurrentPassword");
    }

    [Fact]
    public async Task Handle_PasswordValido_CambiaHashYRetornaSuccess()
    {
        var usuario = new Usuario { Email = "u@x.com", PasswordHash = "oldhash" };
        _usuarios.GetByEmailAsync("u@x.com", Arg.Any<CancellationToken>()).Returns(usuario);
        _hasher.Verify("oldpass", "oldhash").Returns(true);
        _hasher.Hash("newpass1").Returns("newhash");
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(
            new ChangePasswordCommand("u@x.com", "oldpass", "newpass1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        usuario.PasswordHash.Should().Be("newhash");
    }
}
