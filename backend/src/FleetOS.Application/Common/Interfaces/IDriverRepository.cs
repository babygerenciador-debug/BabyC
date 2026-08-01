using FleetOS.Domain.Operations.Drivers;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Application.Operations.Drivers;

namespace FleetOS.Application.Common.Interfaces;

public interface IDriverRepository : IRepository<Driver>
{
    Task<Driver?> GetByCnhAsync(string cnhNumber, CancellationToken cancellationToken = default);
    Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DriverDto?> GetDriverByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DriverDto>> GetPaginatedDriversAsync(int page, int pageSize, string? searchTerm, string? status, CancellationToken cancellationToken = default);
}
