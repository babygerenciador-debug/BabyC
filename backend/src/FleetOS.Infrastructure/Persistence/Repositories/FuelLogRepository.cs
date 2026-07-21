using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Fleet.Fuel;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class FuelLogRepository : IFuelLogRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public FuelLogRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(FuelLog entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<FuelLog>().AddAsync(entity, cancellationToken);
    }

    public void Update(FuelLog entity) => _dbContext.Set<FuelLog>().Update(entity);

    public void Remove(FuelLog entity) => _dbContext.Set<FuelLog>().Remove(entity);

    public async Task<IReadOnlyList<FuelLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<FuelLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId && f.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FuelLogDto?> GetFuelLogByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId && f.Id == id)
            .GroupJoin(_dbContext.Set<FleetOS.Domain.Operations.Drivers.Driver>(),
                f => f.DriverId,
                d => d.Id,
                (f, drivers) => new { f, drivers })
            .SelectMany(
                x => x.drivers.DefaultIfEmpty(),
                (x, d) => new { x.f, d })
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                fd => fd.f.VehicleId,
                v => v.Id,
                (fd, v) => new { fd.f, fd.d, v })
            .GroupJoin(_dbContext.Set<FleetOS.Domain.Core.Users.User>(),
                x => x.d.UserId,
                u => u.Id,
                (x, users) => new { x.f, x.d, x.v, users })
            .SelectMany(
                x => x.users.DefaultIfEmpty(),
                (x, u) => new FuelLogDto(
                    x.f.Id,
                    x.f.VehicleId,
                    x.v.LicensePlate,
                    x.f.DriverId,
                    u != null ? u.Name : null,
                    x.f.Date,
                    x.f.Odometer,
                    x.f.Liters,
                    x.f.TotalCost,
                    x.f.AverageConsumption,
                    x.f.ReceiptUrl,
                    x.f.Notes,
                    x.f.CreatedAt,
                    x.f.UpdatedAt
                ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<FuelLogDto>> GetPaginatedFuelLogsAsync(int page, int pageSize, Guid? vehicleId, Guid? driverId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId);

        if (vehicleId.HasValue)
            query = query.Where(f => f.VehicleId == vehicleId.Value);

        if (driverId.HasValue)
            query = query.Where(f => f.DriverId == driverId.Value);

        if (startDate.HasValue)
            query = query.Where(f => f.Date >= startDate.Value);
            
        if (endDate.HasValue)
            query = query.Where(f => f.Date <= endDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupJoin(_dbContext.Set<FleetOS.Domain.Operations.Drivers.Driver>(),
                f => f.DriverId,
                d => d.Id,
                (f, drivers) => new { f, drivers })
            .SelectMany(
                x => x.drivers.DefaultIfEmpty(),
                (x, d) => new { x.f, d })
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                fd => fd.f.VehicleId,
                v => v.Id,
                (fd, v) => new { fd.f, fd.d, v })
            .GroupJoin(_dbContext.Set<FleetOS.Domain.Core.Users.User>(),
                x => x.d.UserId,
                u => u.Id,
                (x, users) => new { x.f, x.d, x.v, users })
            .SelectMany(
                x => x.users.DefaultIfEmpty(),
                (x, u) => new FuelLogDto(
                    x.f.Id,
                    x.f.VehicleId,
                    x.v.LicensePlate,
                    x.f.DriverId,
                    u != null ? u.Name : null,
                    x.f.Date,
                    x.f.Odometer,
                    x.f.Liters,
                    x.f.TotalCost,
                    x.f.AverageConsumption,
                    x.f.ReceiptUrl,
                    x.f.Notes,
                    x.f.CreatedAt,
                    x.f.UpdatedAt
                ))
            .ToListAsync(cancellationToken);

        return PagedResult<FuelLogDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<FuelLog?> GetLastFuelLogForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId && f.VehicleId == vehicleId)
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Odometer)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
