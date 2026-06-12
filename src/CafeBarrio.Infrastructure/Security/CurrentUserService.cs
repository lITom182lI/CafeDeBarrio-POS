using System.Security.Claims;
using CafeBarrio.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CafeBarrio.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public int? SedeId => int.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirstValue("sede_id"), out var id)
        ? id : null;

    public int? UserId => int.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : null;
}
