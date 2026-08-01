using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

internal sealed class GetMyActiveTripQueryHandler : IRequestHandler<GetMyActiveTripQuery, Result<TripDto?>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ITenantContext _tenantContext;

    public GetMyActiveTripQueryHandler(
        IDriverRepository driverRepository,
        ITripRepository tripRepository,
        ITenantContext tenantContext)
    {
        _driverRepository = driverRepository;
        _tripRepository = tripRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TripDto?>> Handle(GetMyActiveTripQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByUserIdAsync(_tenantContext.UserId, cancellationToken);
        if (driver is null)
            return Result.Success((TripDto?)null);

        var activeTrip = await _tripRepository.GetActiveTripByDriverIdAsync(driver.Id, cancellationToken);
        return Result.Success(activeTrip);
    }
}
