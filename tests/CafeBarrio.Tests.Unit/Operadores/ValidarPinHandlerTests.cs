using System;
using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Operadores;

public class ValidarPinHandlerTests
{
    private readonly IOperadorRepository _operadores = Substitute.For<IOperadorRepository>();
    private readonly IPasswordHasher _hasher         = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt                = Substitute.For<IJwtService>();
    private readonly IUnitOfWork _uow                = Substitute.For<IUnitOfWork>();
    private readonly ValidarPinHandler _sut;

    public ValidarPinHandlerTests() =>
        _sut = new ValidarPinHandler(_operadores, _hasher, _jwt, _uow);

    [Fact]
    public async Task Handle_OperadorNotFound_ReturnsFailure()
    {
        _operadores.GetActivoByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Operador?)null);

        var result = await _sut.Handle(new ValidarPinCommand(99, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Operador.NotFound");
    }

    [Fact]
    public async Task Handle_PinIncorrecto_ReturnsFailure()
    {
        _operadores.GetActivoByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(new Operador { OperadorId = 1, PinHash = "hash" });
        _hasher.Verify("9999", "hash").Returns(false);

        var result = await _sut.Handle(new ValidarPinCommand(1, "9999"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Operador.PinInvalido");
    }

    [Fact]
    public async Task Handle_PinCorrecto_ReturnsTokenDto()
    {
        _operadores.GetActivoByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(new Operador { OperadorId = 1, Nombre = "Ana", PinHash = "hash" });
        _hasher.Verify("1234", "hash").Returns(true);
        _jwt.GenerateOperadorToken(1, "Ana", Arg.Any<string>(), Arg.Any<int>()).Returns("jwt-token");

        var result = await _sut.Handle(new ValidarPinCommand(1, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.Nombre.Should().Be("Ana");
    }

    [Fact]
    public async Task Handle_After5FailedAttempts_LocksOperador()
    {
        var operador = new Operador { OperadorId = 1, PinHash = "hash", Activo = true };
        _operadores.GetActivoByIdAsync(1, Arg.Any<CancellationToken>()).Returns(operador);
        _hasher.Verify(Arg.Any<string>(), "hash").Returns(false);

        for (int i = 0; i < 5; i++)
            await _sut.Handle(new ValidarPinCommand(1, "wrong"), CancellationToken.None);

        operador.IsLockedOut.Should().BeTrue();
        operador.FailedPinAttempts.Should().Be(5);
        operador.LockedUntilUtc.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WhenLockedOut_ReturnsLockedError()
    {
        var operador = new Operador
        {
            OperadorId    = 1,
            PinHash       = "hash",
            Activo        = true,
            IsLockedOut   = true,
            LockedUntilUtc = DateTime.UtcNow.AddMinutes(5)
        };
        _operadores.GetActivoByIdAsync(1, Arg.Any<CancellationToken>()).Returns(operador);

        var result = await _sut.Handle(new ValidarPinCommand(1, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Operador.Locked");
    }
}
