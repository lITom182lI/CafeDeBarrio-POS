using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CafeBarrio.Infrastructure.Persistence.Interceptors;

// SCOPED — se resuelve una vez por request HTTP.
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditInterceptor(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void Stamp(DbContext? ctx)
    {
        if (ctx is null) return;

        var now  = DateTime.UtcNow;
        var user = _currentUser.Email ?? "system";

        foreach (var entry in ctx.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = user;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = user;
            }
        }
    }
}
