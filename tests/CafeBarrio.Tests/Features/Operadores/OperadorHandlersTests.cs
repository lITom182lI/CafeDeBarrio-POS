using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;
using CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;
using CafeBarrio.Application.Features.Operadores.Dtos;
using CafeBarrio.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using MUIS_CORE.Wrappers;
using NSubstitute;

namespace CafeBarrio.Tests.Features.Operadores;

// ─────────────────────────────────────────────────────────────────────────────
//  MUIS_QA_TESTING (Tipo 1) — Estrategia 2B: acceso directo sin HTTP server.
//  Aislamiento total via NSubstitute. Sin fixtures pesados.
// ─────────────────────────────────────────────────────────────────────────────

public class ValidarPinHandlerTests
{
    private readonly IOperadorRepository _repo      = Substitute.For<IOperadorRepository>();
    private readonly IPasswordHasher     _hasher    = Substitute.For<IPasswordHasher>();
    private readonly IJwtService         _jwt       = Substitute.For<IJwtService>();

    private ValidarPinHandler CreateSut() => new(_repo, _hasher, _jwt);

    [Fact]
    public async Task Handle_OperadorActivoConPinCorrecto_DevuelveToken()
    {
        // Arrange
        var operador = new Operador { OperadorId = 1, Nombre = "Ana Garcia", PinHash = "hash_correcto", Activo = true };
        _repo.GetActivoByIdAsync(1, default).Returns(operador);
        _hasher.Verify("1234", "hash_correcto").Returns(true);
        _jwt.GenerateOperadorToken(1, "Ana Garcia").Returns("jwt_token_generado");

        // Act
        var result = await CreateSut().Handle(new ValidarPinCommand(1, "1234"), default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("jwt_token_generado", result.Value!.Token);
        Assert.Equal(1, result.Value.OperadorId);
    }

    [Fact]
    public async Task Handle_PinIncorrecto_DevuelveFailure()
    {
        // Arrange
        var operador = new Operador { OperadorId = 2, Nombre = "Luis Torres", PinHash = "hash_real", Activo = true };
        _repo.GetActivoByIdAsync(2, default).Returns(operador);
        _hasher.Verify("9999", "hash_real").Returns(false);

        // Act
        var result = await CreateSut().Handle(new ValidarPinCommand(2, "9999"), default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Operador.PinInvalido");
    }

    [Fact]
    public async Task Handle_OperadorInactivo_DevuelveNotFound()
    {
        // Arrange — GetActivoByIdAsync devuelve null si el operador está inactivo
        _repo.GetActivoByIdAsync(99, default).Returns((Operador?)null);

        // Act
        var result = await CreateSut().Handle(new ValidarPinCommand(99, "1234"), default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Operador.NotFound");
    }

    [Fact]
    public async Task Handle_OperadorNoExiste_DevuelveNotFound()
    {
        // Arrange
        _repo.GetActivoByIdAsync(999, default).Returns((Operador?)null);

        // Act
        var result = await CreateSut().Handle(new ValidarPinCommand(999, "1234"), default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
    }
}

public class DeleteOperadorHandlerTests
{
    private readonly IOperadorRepository              _repo   = Substitute.For<IOperadorRepository>();
    private readonly IUnitOfWork                      _uow    = Substitute.For<IUnitOfWork>();
    private readonly NullLogger<DeleteOperadorHandler> _logger = new();

    private DeleteOperadorHandler CreateSut() => new(_repo, _uow, _logger);

    [Fact]
    public async Task Handle_OperadorExistente_RealizaSoftDelete()
    {
        // Arrange
        var operador = new Operador { OperadorId = 1, Nombre = "Ana Garcia", Activo = true };
        _repo.GetByIdAsync(1, default).Returns(operador);

        // Act
        var result = await CreateSut().Handle(new DeleteOperadorCommand(1), default);

        // Assert — soft delete verificado
        Assert.True(result.IsSuccess);
        Assert.False(operador.Activo);   // Activo debe ser false después del soft-delete
        await _uow.Received(1).SaveChangesAsync(default); // SaveChanges fue llamado
    }

    [Fact]
    public async Task Handle_OperadorNoExiste_DevuelveNotFound()
    {
        // Arrange
        _repo.GetByIdAsync(999, default).Returns((Operador?)null);

        // Act
        var result = await CreateSut().Handle(new DeleteOperadorCommand(999), default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Operador.NotFound");
        await _uow.DidNotReceive().SaveChangesAsync(default); // No guardó nada
    }

    [Fact]
    public async Task Handle_OperadorYaInactivo_RealizaSoftDeleteIgualmente()
    {
        // Escenario extremo: operador ya estaba inactivo pero existe en DB
        var operador = new Operador { OperadorId = 2, Nombre = "Luis Torres", Activo = false };
        _repo.GetByIdAsync(2, default).Returns(operador);

        // Act
        var result = await CreateSut().Handle(new DeleteOperadorCommand(2), default);

        // Assert — la operación igual es exitosa (idempotente)
        Assert.True(result.IsSuccess);
        await _uow.Received(1).SaveChangesAsync(default);
    }
}
