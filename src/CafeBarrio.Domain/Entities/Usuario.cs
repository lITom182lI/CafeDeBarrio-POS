namespace CafeBarrio.Domain.Entities;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Admin";
    public bool Activo { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;
    public DateTime? LockedUntil { get; set; }
}
