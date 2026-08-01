using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

internal sealed class GetDriverTripsQueryHandler : IRequestHandler<GetDriverTripsQuery, Result<PagedResult<TripDto>>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ITenantContext _tenantContext;

    public GetDriverTripsQueryHandler(
        IDriverRepository driverRepository,
        ITripRepository tripRepository,
        ITenantContext tenantContext)
    {
        _driverRepository = driverRepository;
        _tripRepository = tripRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<TripDto>>> Handle(GetDriverTripsQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByUserIdAsync(_tenantContext.UserId, cancellationToken);
        if (driver is null)
            return Result.Success(PagedResult<TripDto>.Create(new List<TripDto>(), 0, 1, 50));

        var trips = await _tripRepository.GetTripsByDriverIdAsync(driver.Id, cancellationToken: cancellationToken);
        return Result.Success(PagedResult<TripDto>.Create(trips.ToList(), trips.Count, 1, 50));
    }
}
