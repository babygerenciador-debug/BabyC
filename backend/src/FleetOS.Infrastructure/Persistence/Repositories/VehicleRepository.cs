using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Fleet.Vehicles;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Domain.Core.Users;
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

    public async Task<VehicleDto?> GetVehicleByIdWithDriverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = from v in DbSet
                    join d in DbContext.Drivers on v.AssignedDriverId equals (Guid?)d.Id into drivers
                    from d in drivers.DefaultIfEmpty()
                    join u in DbContext.Users on d.UserId equals u.Id into users
                    from u in users.DefaultIfEmpty()
                    where v.Id == id
                    select new VehicleDto(
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
                        u != null ? u.Name : null,
                        u != null ? u.CpfLast4 : null,
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
                    );

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<VehicleDto>> GetPaginatedVehiclesAsync(
        int page, int pageSize, string? searchTerm, string? status, CancellationToken cancellationToken = default)
    {
        var query = from v in DbSet
                    join d in DbContext.Drivers on v.AssignedDriverId equals (Guid?)d.Id into drivers
                    from d in drivers.DefaultIfEmpty()
                    join u in DbContext.Users on d.UserId equals u.Id into users
                    from u in users.DefaultIfEmpty()
                    select new { v, d, u };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(x =>
                x.v.LicensePlate.ToLower().Contains(lowerSearch) ||
                (x.v.Chassi != null && x.v.Chassi.ToLower().Contains(lowerSearch)) ||
                (x.v.Model != null && x.v.Model.ToLower().Contains(lowerSearch)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VehicleStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.v.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new VehicleDto(
                x.v.Id,
                x.v.LicensePlate,
                x.v.Chassi,
                x.v.Nickname,
                x.v.Brand,
                x.v.Color,
                x.v.Model,
                x.v.Capacity,
                x.v.Year,
                x.v.PhotoUrl,
                x.v.Status.ToString(),
                x.v.AssignedDriverId,
                x.u != null ? x.u.Name : null,
                x.u != null ? x.u.CpfLast4 : null,
                x.v.Renavam,
                x.v.AnttNumber,
                x.v.AnttExpiry,
                x.v.ArtespExpiry,
                x.v.InsuranceExpiry,
                x.v.LicensingExpiry,
                x.v.FuelAlertMode != null ? x.v.FuelAlertMode.ToString() : null,
                x.v.FuelAlertDays,
                x.v.LastFuelAt,
                x.v.CurrentOdometerKm,
                x.v.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<VehicleDto>.Create(items, totalCount, page, pageSize);
    }
}
