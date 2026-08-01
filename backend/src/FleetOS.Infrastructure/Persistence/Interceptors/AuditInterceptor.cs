using FleetOS.Domain.Common;
using FleetOS.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FleetOS.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically fills audit fields
/// (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy)
/// before every SaveChanges call. No need to set these manually.
/// ICurrentUserService is defined in FleetOS.Domain.Common.Interfaces (DIP-compliant).
/// </summary>
public sealed class AuditInterceptor(ICurrentUserService currentUserService)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var userId = currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedBy(userId ?? Guid.Empty);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedBy(userId ?? Guid.Empty);
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(userId ?? Guid.Empty);
                    break;
            }
        }
    }
}
