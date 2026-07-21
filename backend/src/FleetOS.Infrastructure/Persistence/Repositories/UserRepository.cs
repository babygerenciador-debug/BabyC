using FleetOS.Domain.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(FleetOsDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Auth query: must bypass the global tenant filter because during login
        // no JWT exists yet, so _currentTenantId is Guid.Empty and the filter
        // would block all users. We filter manually for correctness.
        return DbContext.Users
            .Include(u => u.RefreshTokens)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EmailAddress == email && u.DeletedAt == null, cancellationToken);
    }

    public Task<User?> GetByCpfHashAsync(Guid tenantId, string cpfHash, CancellationToken cancellationToken = default)
    {
        // We override the implicit global query filter tenant check to make it explicit, 
        // but EF will append the global filter anyway (TenantId == CurrentTenantId).
        // Since driver login happens before we have a JWT, the global filter might be empty (Guid.Empty) 
        // if we don't bypass it. For auth queries like this, we should ignore global query filters
        // and manually filter by the identified tenant.
        
        return DbContext.Users
            .Include(u => u.RefreshTokens)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.CpfHash == cpfHash && u.DeletedAt == null, cancellationToken);
    }
    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return DbContext.Users
            .Include(u => u.RefreshTokens)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == refreshToken), cancellationToken);
    }
}
