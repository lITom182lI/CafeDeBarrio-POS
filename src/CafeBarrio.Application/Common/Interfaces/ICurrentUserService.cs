namespace CafeBarrio.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? Email  { get; }
    int?    SedeId { get; }
    int?    UserId { get; }
}
