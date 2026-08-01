using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.VehicleIssues;

using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class VehicleIssueReportRepository : IVehicleIssueReportRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public VehicleIssueReportRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(VehicleIssueReport entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<VehicleIssueReport>().AddAsync(entity, cancellationToken);

    public void Update(VehicleIssueReport entity) => _dbContext.Set<VehicleIssueReport>().Update(entity);
    public void Remove(VehicleIssueReport entity) => _dbContext.Set<VehicleIssueReport>().Remove(entity);

    public async Task<IReadOnlyList<VehicleIssueReport>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<VehicleIssueReport>().Where(r => r.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<VehicleIssueReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<VehicleIssueReport>().FirstOrDefaultAsync(r => r.TenantId == _tenantContext.TenantId && r.Id == id, cancellationToken);
}
