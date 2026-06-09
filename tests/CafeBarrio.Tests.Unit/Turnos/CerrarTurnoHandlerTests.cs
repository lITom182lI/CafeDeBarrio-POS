using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Turnos;

public class CerrarTurnoHandlerTests
{
    private readonly ITurnoRepository _turnos = Substitute.For<ITurnoRepository>();
    private readonly IUnitOfWork _uow         = Substitute.For<IUnitOfWork>();
    private readonly CerrarTurnoHandler _sut;

    public CerrarTurnoHandlerTests()
    {
        _sut = new CerrarTurnoHandler(_turnos, _uow);
    }

    [Fact]
    public async Task Handle_TurnoNotFound_ReturnsFailure()
    {
        _turnos.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Turno?)null);

        var result = await _sut.Handle(new CerrarTurnoCommand(99, 500m, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Turno.NotFound");
    }

    [Fact]
    public async Task Handle_TurnoNoPuedesCerrar_ReturnsFailure()
    {
        _turnos.GetByIdAsync(1, Arg.Any<CancellationToken>())
               .Returns(new Turno { TurnoId = 1, Estado = "Cerrado" });

        var result = await _sut.Handle(new CerrarTurnoCommand(1, 500m, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Turno.NoPuedesCerrar");
    }

    [Fact]
    public async Task Handle_ValidClose_ReturnsSuccess()
    {
        var turno = new Turno { TurnoId = 1, Estado = "Abierto", MontoApertura = 200m };
        _turnos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(turno);
        _turnos.GetTotalEfectivoByTurnoAsync(1, Arg.Any<CancellationToken>()).Returns(300m);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(new CerrarTurnoCommand(1, 480m, "Sin novedad"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalEfectivoSistema.Should().Be(500m);
        result.Value!.MontoEfectivoCierto.Should().Be(480m);
        result.Value!.Diferencia.Should().Be(-20m);
        turno.Estado.Should().Be("Cerrado");
        turno.TotalEfectivoSistema.Should().Be(500m); // 200 apertura + 300 efectivo
        turno.MontoEfectivoCierto.Should().Be(480m);
    }
}
