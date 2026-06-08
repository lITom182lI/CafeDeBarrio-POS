using CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
using CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Turnos;

[Trait("Category", "Integration")]
public class TurnosIntegrationTests : IntegrationTestBase
{
    private readonly AbrirTurnoHandler _abrirTurno;
    private readonly CerrarTurnoHandler _cerrarTurno;

    public TurnosIntegrationTests() : base()
    {
        var turnosRepo  = new TurnoRepository(Db);
        var uow         = new CafeBarrio.Infrastructure.Persistence.UnitOfWork(Db);
        _abrirTurno  = new AbrirTurnoHandler(turnosRepo, uow);
        _cerrarTurno = new CerrarTurnoHandler(turnosRepo, uow);
    }

    private async Task<(int sId, int oId)> SeedSedeAndOperador()
    {
        var sede = new CafeBarrio.Domain.Entities.Sede { Nombre = "Test", Direccion = "D", Distrito = "D", Ciudad = "C" };
        Db.Sedes.Add(sede);
        await Db.SaveChangesAsync();
        
        var op = new CafeBarrio.Domain.Entities.Operador { SedeId = sede.SedeId, Nombre = "O", PinHash = "1", Activo = true };
        Db.Operadores.Add(op);
        await Db.SaveChangesAsync();
        return (sede.SedeId, op.OperadorId);
    }

    [Fact]
    public async Task AbrirTurno_SinTurnoActivo_CreaYRetornaId()
    {
        var (sId, oId) = await SeedSedeAndOperador();
        var result = await _abrirTurno.Handle(
            new AbrirTurnoCommand(sId, oId, 200m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AbrirTurno_ConTurnoActivo_RetornaYaAbierto()
    {
        var (sId, oId) = await SeedSedeAndOperador();
        // Abrir primero
        await _abrirTurno.Handle(
            new AbrirTurnoCommand(sId, oId, 100m), CancellationToken.None);

        // Intentar abrir de nuevo
        var result = await _abrirTurno.Handle(
            new AbrirTurnoCommand(sId, oId, 100m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Turno.YaAbierto");
    }

    [Fact]
    public async Task CerrarTurno_TurnoAbierto_CierraCorrecto()
    {
        var (sId, oId) = await SeedSedeAndOperador();
        var abierto = await _abrirTurno.Handle(
            new AbrirTurnoCommand(sId, oId, 300m), CancellationToken.None);
        abierto.IsSuccess.Should().BeTrue();

        var result = await _cerrarTurno.Handle(
            new CerrarTurnoCommand(abierto.Value, 310m, "Sin novedad"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
