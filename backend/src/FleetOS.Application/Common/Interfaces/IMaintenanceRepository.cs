using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Maintenance;
using FleetOS.Shared.Pagination;
using FleetOS.Application.Fleet.Maintenance;

namespace FleetOS.Application.Common.Interfaces;

public interface IMaintenanceRepository : IRepository<MaintenanceRecord>
{
    Task<MaintenanceDto?> GetMaintenanceByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<MaintenanceDto>> GetPaginatedMaintenancesAsync(int page, int pageSize, Guid? vehicleId, MaintenanceType? type, MaintenanceStatus? status, CancellationToken cancellationToken = default);
}
