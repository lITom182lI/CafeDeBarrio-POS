using CafeBarrio.API.Controllers;
using CafeBarrio.Application.Features.Auth.Dtos;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Auth;

[Trait("Category", "Integration")]
public class AuthIntegrationTests : IntegrationTestBase
{
    private readonly AuthController _controller;

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
        _controller = new AuthController(usuariosRepo, jwtService, hasher, null!);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.Hash("password123");
        var usuario = new Usuario { Email = "admin@cafebarrio.pe", PasswordHash = hash, Rol = "Administrador", Activo = true };
        Db.Usuarios.Add(usuario);
        await Db.SaveChangesAsync();

        var request = new LoginRequest("admin@cafebarrio.pe", "password123");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult!.Value as LoginResponse;
        response.Should().NotBeNull();
        response!.Token.Should().NotBeNullOrEmpty();
        response.Email.Should().Be("admin@cafebarrio.pe");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@cafebarrio.pe", "wrongpassword");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
    }
}
