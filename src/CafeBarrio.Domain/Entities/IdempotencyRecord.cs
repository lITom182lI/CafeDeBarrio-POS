namespace CafeBarrio.Domain.Entities;

public sealed class IdempotencyRecord
{
    public int    Id              { get; set; }
    public string IdempotencyKey  { get; set; } = default!;
    public int    TransaccionId   { get; set; }
    public DateTime CreatedAtUtc  { get; set; }
}
