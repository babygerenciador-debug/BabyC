using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Fleet.Maintenance;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Maintenance;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class MaintenanceRepository : IMaintenanceRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public MaintenanceRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(MaintenanceRecord entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<MaintenanceRecord>().AddAsync(entity, cancellationToken);
    }

    public void Update(MaintenanceRecord entity) => _dbContext.Set<MaintenanceRecord>().Update(entity);

    public void Remove(MaintenanceRecord entity) => _dbContext.Set<MaintenanceRecord>().Remove(entity);

    public async Task<IReadOnlyList<MaintenanceRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<MaintenanceRecord>()
            .Where(m => m.TenantId == _tenantContext.TenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<MaintenanceRecord>()
            .Where(m => m.TenantId == _tenantContext.TenantId && m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MaintenanceDto?> GetMaintenanceByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<MaintenanceRecord>()
            .Where(m => m.TenantId == _tenantContext.TenantId && m.Id == id)
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                m => m.VehicleId,
                v => v.Id,
                (m, v) => new MaintenanceDto(
                    m.Id,
                    m.VehicleId,
                    v.LicensePlate,
                    m.Type,
                    m.Status,
                    m.Date,
                    m.Odometer,
                    m.Description,
                    m.TotalCost,
                    m.ProviderName,
                    m.InvoiceUrl,
                    m.Notes,
                    m.CreatedAt,
                    m.UpdatedAt
                ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<MaintenanceDto>> GetPaginatedMaintenancesAsync(int page, int pageSize, Guid? vehicleId, MaintenanceType? type, MaintenanceStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<MaintenanceRecord>()
            .Where(m => m.TenantId == _tenantContext.TenantId);

        if (vehicleId.HasValue)
            query = query.Where(m => m.VehicleId == vehicleId.Value);

        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                m => m.VehicleId,
                v => v.Id,
                (m, v) => new MaintenanceDto(
                    m.Id,
                    m.VehicleId,
                    v.LicensePlate,
                    m.Type,
                    m.Status,
                    m.Date,
                    m.Odometer,
                    m.Description,
                    m.TotalCost,
                    m.ProviderName,
                    m.InvoiceUrl,
                    m.Notes,
                    m.CreatedAt,
                    m.UpdatedAt
                ))
            .ToListAsync(cancellationToken);

        return PagedResult<MaintenanceDto>.Create(items, totalCount, page, pageSize);
    }
}
