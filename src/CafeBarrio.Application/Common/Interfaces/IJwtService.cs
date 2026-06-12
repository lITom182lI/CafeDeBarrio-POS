using CafeBarrio.Domain.Entities;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
    string GenerateOperadorToken(int operadorId, string nombre, string securityStamp, int sedeId);
}
