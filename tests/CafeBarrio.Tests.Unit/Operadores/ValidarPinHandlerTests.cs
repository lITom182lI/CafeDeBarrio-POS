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
    private readonly ValidarPinHandler _sut;

    public ValidarPinHandlerTests() =>
        _sut = new ValidarPinHandler(_operadores, _hasher, _jwt);

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
        _jwt.GenerateOperadorToken(1, "Ana").Returns("jwt-token");

        var result = await _sut.Handle(new ValidarPinCommand(1, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.Nombre.Should().Be("Ana");
    }
}
