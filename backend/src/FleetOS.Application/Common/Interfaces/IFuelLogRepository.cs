using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Shared.Pagination;
using FleetOS.Application.Fleet.Fuel;

namespace FleetOS.Application.Common.Interfaces;

public interface IFuelLogRepository : IRepository<FuelLog>
{
    Task<FuelLogDto?> GetFuelLogByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<FuelLogDto>> GetPaginatedFuelLogsAsync(int page, int pageSize, Guid? vehicleId, Guid? driverId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<FuelLog?> GetLastFuelLogForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
