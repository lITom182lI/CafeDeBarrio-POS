using CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;
using CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Operadores;

[Trait("Category", "Integration")]
public class OperadoresIntegrationTests : IntegrationTestBase
{
    private readonly ValidarPinHandler _validarPin;
    private readonly CreateOperadorHandler _createOperador;
    private readonly Argon2PasswordHasher _hasher;

    public OperadoresIntegrationTests() : base()
    {
        var operadoresRepo = new OperadorRepository(Db);
        var uow            = new CafeBarrio.Infrastructure.Persistence.UnitOfWork(Db);
        var sedesRepo      = new SedeRepository(Db);
        _hasher            = new Argon2PasswordHasher();

        // JwtService solo para ValidarPin — config mínima
        var jwtService = new JwtService(Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Key = "TEST_SECRET_KEY_FOR_INTEGRATION_2026!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        }));

        _validarPin    = new ValidarPinHandler(operadoresRepo, _hasher, jwtService, uow);
        _createOperador = new CreateOperadorHandler(operadoresRepo, sedesRepo, uow, _hasher);
    }

    private async Task EnsureSede1Exists()
    {
        if (!await Db.Sedes.AnyAsync(s => s.SedeId == 1))
        {
            await Db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Sede ON; INSERT INTO Sede (sede_id, nombre, direccion, distrito, ciudad, es_principal, activa) VALUES (1, 'Sede Central', 'Dir', 'Dist', 'Ciu', 1, 1); SET IDENTITY_INSERT Sede OFF;");
        }
    }

    [Fact]
    public async Task ValidarPin_OperadorNoExiste_RetornaFailure()
    {
        var result = await _validarPin.Handle(new ValidarPinCommand(9999, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Operador.NotFound");
    }

    [Fact]
    public async Task ValidarPin_PinCorrecto_RetornaToken()
    {
        await EnsureSede1Exists();

        // Crear operador en BD de test
        var created = await _createOperador.Handle(
            new CreateOperadorCommand("Barista Test", "432100"), CancellationToken.None);
        created.IsSuccess.Should().BeTrue();

        // Validar pin
        var result = await _validarPin.Handle(
            new ValidarPinCommand(created.Value, "432100"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeNullOrEmpty();
        result.Value.Nombre.Should().Be("Barista Test");
    }
}
