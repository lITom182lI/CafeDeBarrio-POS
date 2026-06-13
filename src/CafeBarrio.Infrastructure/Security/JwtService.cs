using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CafeBarrio.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    public JwtService(IOptions<JwtOptions> options) => _options = options.Value;

    public string GenerateToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email,           usuario.Email),
            new Claim(ClaimTypes.Role,            usuario.Rol),
            new Claim("security_stamp",           usuario.SecurityStamp),
        };
        return BuildToken(claims, _options.AdminExpiryHours);
    }

    public string GenerateOperadorToken(int operadorId, string nombre, string securityStamp, int sedeId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, operadorId.ToString()),
            new Claim(ClaimTypes.Name,           nombre),
            new Claim(ClaimTypes.Role,           "Operador"),
            new Claim("security_stamp",          securityStamp),
            new Claim("sede_id",                 sedeId.ToString()),
        };
        return BuildToken(claims, _options.OperadorExpiryHours);
    }

    private string BuildToken(Claim[] claims, int hours)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _options.Issuer,
            audience:           _options.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
