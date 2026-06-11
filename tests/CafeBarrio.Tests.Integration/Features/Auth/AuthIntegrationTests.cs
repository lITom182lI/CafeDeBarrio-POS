using CafeBarrio.Application.Features.Auth.Commands.Login;
using CafeBarrio.Application.Features.Auth.Dtos;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Auth;

[Trait("Category", "Integration")]
public class AuthIntegrationTests : IntegrationTestBase
{
    private readonly LoginCommandHandler _handler;

    public AuthIntegrationTests() : base()
    {
        var usuariosRepo = new UsuarioRepository(Db);
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "SUPER_SECRET_KEY_FOR_TESTING_PURPOSES_2026!"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryHours", "8"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        
        var jwtService = new JwtService(config);
        var hasher = new Argon2PasswordHasher();
        var uow = new UnitOfWork(Db);
        _handler = new LoginCommandHandler(usuariosRepo, jwtService, hasher, uow);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.Hash("password123");
        var usuario = new Usuario { Email = "admin@cafebarrio.pe", PasswordHash = hash, Rol = "Administrador", Activo = true };
        Db.Usuarios.Add(usuario);
        await Db.SaveChangesAsync();

        var command = new LoginCommand("admin@cafebarrio.pe", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().NotBeNullOrEmpty();
        result.Value.Email.Should().Be("admin@cafebarrio.pe");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@cafebarrio.pe", "wrongpassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Auth.InvalidCredentials");
    }
}
