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
}
