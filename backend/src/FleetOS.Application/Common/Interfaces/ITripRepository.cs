using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Trips;
using FleetOS.Shared.Pagination;
using FleetOS.Application.Operations.Trips;

namespace FleetOS.Application.Common.Interfaces;

public interface ITripRepository : IRepository<Trip>
{
    Task<TripDto?> GetTripByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TripDto>> GetPaginatedTripsAsync(int page, int pageSize, string? searchTerm, string? status, Guid? driverId, Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<TripDto?> GetActiveTripByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TripDto>> GetTripsByDriverIdAsync(Guid driverId, int take = 50, CancellationToken cancellationToken = default);
}
