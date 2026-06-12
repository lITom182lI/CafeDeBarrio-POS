using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Turnos;

public class AbrirTurnoHandlerTests
{
    private readonly ITurnoRepository _turnos = Substitute.For<ITurnoRepository>();
    private readonly IUnitOfWork _uow         = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly AbrirTurnoHandler _sut;

    public AbrirTurnoHandlerTests() => _sut = new AbrirTurnoHandler(_turnos, _uow, _currentUser);

    [Fact]
    public async Task Handle_TurnoYaAbierto_ReturnsFailure()
    {
        _turnos.GetActivoBySedeAsync(1, Arg.Any<CancellationToken>())
               .Returns(new Turno { TurnoId = 1, Estado = "Abierto" });

        var result = await _sut.Handle(new AbrirTurnoCommand(1, 2, 100m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Turno.YaAbierto");
    }

    [Fact]
    public async Task Handle_NoHayTurnoActivo_CreaTurnoYRetornaId()
    {
        _turnos.GetActivoBySedeAsync(1, Arg.Any<CancellationToken>()).Returns((Turno?)null);
        _turnos.AddAsync(Arg.Any<Turno>(), Arg.Any<CancellationToken>())
               .Returns(Result<int>.Success(10));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(new AbrirTurnoCommand(1, 2, 100m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }
}
