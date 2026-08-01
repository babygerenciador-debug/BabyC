using FleetOS.Domain.Core.Tenants;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(FleetOsDbContext dbContext) : BaseRepository<Tenant>(dbContext), ITenantRepository
{
    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // Tenant is the root, no global query filter blocks it, just check soft delete
        return DbContext.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }
}
