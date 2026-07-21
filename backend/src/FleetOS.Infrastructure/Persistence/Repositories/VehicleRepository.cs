using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Fleet.Vehicles;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(FleetOsDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, cancellationToken);
    }

    public async Task<Vehicle?> GetByChassiAsync(string chassi, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(v => v.Chassi == chassi, cancellationToken);
    }

    public async Task<PagedResult<VehicleDto>> GetPaginatedVehiclesAsync(
        int page, int pageSize, string? searchTerm, string? status, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(v => 
                v.LicensePlate.ToLower().Contains(lowerSearch) || 
                (v.Chassi != null && v.Chassi.ToLower().Contains(lowerSearch)) ||
                (v.Model != null && v.Model.ToLower().Contains(lowerSearch)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VehicleStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(v => v.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VehicleDto(
                v.Id,
                v.LicensePlate,
                v.Chassi,
                v.Nickname,
                v.Brand,
                v.Color,
                v.Model,
                v.Capacity,
                v.Year,
                v.PhotoUrl,
                v.Status.ToString(),
                v.AssignedDriverId,
                v.Renavam,
                v.AnttNumber,
                v.AnttExpiry,
                v.ArtespExpiry,
                v.InsuranceExpiry,
                v.LicensingExpiry,
                v.FuelAlertMode != null ? v.FuelAlertMode.ToString() : null,
                v.FuelAlertDays,
                v.LastFuelAt,
                v.CurrentOdometerKm,
                v.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<VehicleDto>.Create(items, totalCount, page, pageSize);
    }
}
