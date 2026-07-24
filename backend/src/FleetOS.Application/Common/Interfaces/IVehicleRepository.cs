using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Application.Fleet.Vehicles;
using FleetOS.Shared.Pagination;

namespace FleetOS.Application.Common.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByChassiAsync(string chassi, CancellationToken cancellationToken = default);
    Task<PagedResult<VehicleDto>> GetPaginatedVehiclesAsync(int page, int pageSize, string? searchTerm, string? status, CancellationToken cancellationToken = default);
    Task<VehicleDto?> GetVehicleByIdWithDriverAsync(Guid id, CancellationToken cancellationToken = default);
}
