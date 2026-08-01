using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Common.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : INotificationRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public NotificationRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Notification entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Notification>().AddAsync(entity, cancellationToken);

    public void Update(Notification entity) => _dbContext.Set<Notification>().Update(entity);
    public void Remove(Notification entity) => _dbContext.Set<Notification>().Remove(entity);

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<Notification>().Where(n => n.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Notification>().FirstOrDefaultAsync(n => n.TenantId == _tenantContext.TenantId && n.Id == id, cancellationToken);

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid? userId, string[] roles, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        
        return await _dbContext.Set<Notification>()
            .Where(n => n.TenantId == tenantId && !n.IsRead && 
                        (n.UserId == userId || (n.Role != null && roles.Contains(n.Role))))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
