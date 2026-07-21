using FleetOS.Domain.Common.Interfaces;

namespace FleetOS.Infrastructure.Persistence;

public sealed class UnitOfWork(FleetOsDbContext dbContext) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CommitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // AuditInterceptor already reads the current user from ICurrentUserService
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
