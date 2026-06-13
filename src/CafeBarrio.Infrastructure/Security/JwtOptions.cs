using System.ComponentModel.DataAnnotations;

namespace CafeBarrio.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string Key { get; init; } = "";

    [Required]
    public string Issuer { get; init; } = "";

    [Required]
    public string Audience { get; init; } = "";

    public int ExpiryHours         { get; init; } = 8;
    public int AdminExpiryHours    { get; init; } = 4;
    public int OperadorExpiryHours { get; init; } = 12;
}
