using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Operations.Trips;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Trips;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository : ITripRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public TripRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Trip entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Trip>().AddAsync(entity, cancellationToken);
    }

    public void Update(Trip entity) => _dbContext.Set<Trip>().Update(entity);

    public void Remove(Trip entity) => _dbContext.Set<Trip>().Remove(entity);

    public async Task<IReadOnlyList<Trip>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Trip>()
            .Where(t => t.TenantId == _tenantContext.TenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Trip>()
            .Where(t => t.TenantId == _tenantContext.TenantId && t.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TripDto?> GetTripByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Trip>()
            .Where(t => t.TenantId == _tenantContext.TenantId && t.Id == id)
            .Join(_dbContext.Set<FleetOS.Domain.Operations.Drivers.Driver>(),
                t => t.DriverId,
                d => d.Id,
                (t, d) => new { t, d })
            .Join(_dbContext.Set<FleetOS.Domain.Core.Users.User>(),
                td => td.d.UserId,
                u => u.Id,
                (td, u) => new { td.t, td.d, u })
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                tdu => tdu.t.VehicleId,
                v => v.Id,
                (tdu, v) => new TripDto(
                    tdu.t.Id,
                    tdu.t.DriverId,
                    tdu.u.Name,
                    tdu.t.VehicleId,
                    v.LicensePlate,
                    tdu.t.Origin,
                    tdu.t.Destination,
                    tdu.t.ScheduledStartDate,
                    tdu.t.ScheduledEndDate,
                    tdu.t.TripValue,
                    tdu.t.PaymentStatus.ToString(),
                    tdu.t.Notes,
                    tdu.t.ActualStartDate,
                    tdu.t.ActualEndDate,
                    tdu.t.ChecklistCompleted,
                    tdu.t.ChecklistNotes,
                    tdu.t.Status.ToString(),
                    tdu.t.CreatedAt,
                    tdu.t.UpdatedAt
                ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<TripDto>> GetPaginatedTripsAsync(int page, int pageSize, string? searchTerm, string? status, Guid? driverId, Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Trip>()
            .Where(t => t.TenantId == _tenantContext.TenantId);

        if (driverId.HasValue)
            query = query.Where(t => t.DriverId == driverId.Value);

        if (vehicleId.HasValue)
            query = query.Where(t => t.VehicleId == vehicleId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TripStatus>(status, true, out var parsedStatus))
            query = query.Where(t => t.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Origin.Contains(searchTerm) || t.Destination.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.ScheduledStartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_dbContext.Set<FleetOS.Domain.Operations.Drivers.Driver>(),
                t => t.DriverId,
                d => d.Id,
                (t, d) => new { t, d })
            .Join(_dbContext.Set<FleetOS.Domain.Core.Users.User>(),
                td => td.d.UserId,
                u => u.Id,
                (td, u) => new { td.t, td.d, u })
            .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(),
                tdu => tdu.t.VehicleId,
                v => v.Id,
                (tdu, v) => new TripDto(
                    tdu.t.Id,
                    tdu.t.DriverId,
                    tdu.u.Name,
                    tdu.t.VehicleId,
                    v.LicensePlate,
                    tdu.t.Origin,
                    tdu.t.Destination,
                    tdu.t.ScheduledStartDate,
                    tdu.t.ScheduledEndDate,
                    tdu.t.TripValue,
                    tdu.t.PaymentStatus.ToString(),
                    tdu.t.Notes,
                    tdu.t.ActualStartDate,
                    tdu.t.ActualEndDate,
                    tdu.t.ChecklistCompleted,
                    tdu.t.ChecklistNotes,
                    tdu.t.Status.ToString(),
                    tdu.t.CreatedAt,
                    tdu.t.UpdatedAt
                ))
            .ToListAsync(cancellationToken);

        return PagedResult<TripDto>.Create(items, totalCount, page, pageSize);
    }
}
