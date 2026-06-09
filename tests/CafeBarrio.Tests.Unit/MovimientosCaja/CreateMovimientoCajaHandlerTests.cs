using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;
using MUIS_CORE.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace CafeBarrio.Tests.Unit.MovimientosCaja;

public class CreateMovimientoCajaHandlerTests
{
    private readonly IMovimientoCajaRepository _movimientos;
    private readonly ITurnoRepository _turnos;
    private readonly IUnitOfWork _uow;
    private readonly CreateMovimientoCajaHandler _sut;

    public CreateMovimientoCajaHandlerTests()
    {
        _movimientos = Substitute.For<IMovimientoCajaRepository>();
        _turnos = Substitute.For<ITurnoRepository>();
        _uow = Substitute.For<IUnitOfWork>();

        _sut = new CreateMovimientoCajaHandler(_movimientos, _turnos, _uow);
    }

    [Theory]
    [InlineData("ingreso")]
    [InlineData("EGRESO")]
    [InlineData("Otro")]
    public async Task Handle_TipoInvalido_ReturnsFailure(string tipoInvalido)
    {
        // Arrange
        var command = new CreateMovimientoCajaCommand(1, tipoInvalido, "Motivo", 100);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MovimientoCaja.TipoInvalido");
    }

    [Fact]
    public async Task Handle_MontoNegativo_ReturnsFailure()
    {
        // Arrange
        var command = new CreateMovimientoCajaCommand(1, "Ingreso", "Motivo", -50);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MovimientoCaja.MontoInvalido");
    }

    [Fact]
    public async Task Handle_TurnoNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new CreateMovimientoCajaCommand(1, "Ingreso", "Motivo", 100);
        _turnos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Turno?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MovimientoCaja.TurnoNotFound");
    }

    [Fact]
    public async Task Handle_TurnoCerrado_ReturnsFailure()
    {
        // Arrange
        var command = new CreateMovimientoCajaCommand(1, "Ingreso", "Motivo", 100);
        var turno = new Turno { TurnoId = 1, Estado = "Cerrado" };
        _turnos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(turno);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MovimientoCaja.TurnoCerrado");
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateMovimientoCajaCommand(1, "Ingreso", "Motivo", 100);
        var turno = new Turno { TurnoId = 1, Estado = "Abierto" };
        _turnos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(turno);

        _movimientos.AddAsync(Arg.Any<MovimientoCaja>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(10));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
