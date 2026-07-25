using FleetOS.Domain.Common.Interfaces;

namespace FleetOS.Infrastructure.Persistence;

public sealed class UnitOfWork(FleetOsDbContext dbContext) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CommitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        dbContext.SetTenantId(tenantId);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CommitAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        dbContext.SetTenantId(tenantId, userId);
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
