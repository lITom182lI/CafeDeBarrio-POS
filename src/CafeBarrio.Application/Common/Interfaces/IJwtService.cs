using CafeBarrio.Domain.Entities;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
}
