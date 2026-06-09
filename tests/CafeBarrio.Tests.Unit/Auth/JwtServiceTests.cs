using System;
using System.Collections.Generic;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace CafeBarrio.Tests.Unit.Auth;

public class JwtServiceTests
{
    private static JwtService BuildSut() =>
        new JwtService(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]                 = "super-secret-key-minimo-32-caracteres!",
                ["Jwt:Issuer"]              = "CafeBarrio",
                ["Jwt:Audience"]            = "CafeBarrioApp",
                ["Jwt:ExpiryHours"]         = "8",
                ["Jwt:AdminExpiryHours"]    = "8",
                ["Jwt:OperadorExpiryHours"] = "16"
            })
            .Build());

    [Fact]
    public void GenerateToken_ReturnsValidJwtWithUserClaims()
    {
        var sut    = BuildSut();
        var usuario = new Usuario { UsuarioId = 5, Email = "admin@test.com", Rol = "Admin" };

        var token  = sut.GenerateToken(usuario);
        var jwt    = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "admin@test.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role  && c.Value == "Admin");
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateOperadorToken_ReturnsValidJwtWithOperadorRole()
    {
        var sut   = BuildSut();

        var token = sut.GenerateOperadorToken(operadorId: 10, nombre: "Carlos");
        var jwt   = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Carlos");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Operador");
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(16), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ReturnDifferentTokens()
    {
        var sut  = BuildSut();
        var u1   = new Usuario { UsuarioId = 1, Email = "a@test.com", Rol = "Admin" };
        var u2   = new Usuario { UsuarioId = 2, Email = "b@test.com", Rol = "Cajero" };

        sut.GenerateToken(u1).Should().NotBe(sut.GenerateToken(u2));
    }
}
