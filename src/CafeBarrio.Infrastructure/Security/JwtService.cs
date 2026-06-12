using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CafeBarrio.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("security_stamp", usuario.SecurityStamp),
        };
        // Admin JWT: 4 horas (MUIS_SECURITY_AUTH — Access Token de corta duración para Tipo 1)
        var adminHours = int.TryParse(_config["Jwt:AdminExpiryHours"], out var ah) ? ah : 4;
        return BuildToken(claims, hoursOverride: adminHours);
    }

    public string GenerateOperadorToken(int operadorId, string nombre, string securityStamp, int sedeId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, operadorId.ToString()),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, "Operador"),
            new Claim("security_stamp", securityStamp),
            new Claim("sede_id", sedeId.ToString()),
        };
        var opHours = int.TryParse(_config["Jwt:OperadorExpiryHours"], out var oh) ? oh : 12;
        return BuildToken(claims, hoursOverride: opHours);
    }

    private string BuildToken(Claim[] claims, int? hoursOverride)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        int hours = hoursOverride
            ?? (int.TryParse(_config["Jwt:ExpiryHours"], out var h) ? h : 8);

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
