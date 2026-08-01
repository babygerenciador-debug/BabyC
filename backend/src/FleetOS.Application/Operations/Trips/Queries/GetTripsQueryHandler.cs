using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

internal sealed class GetTripsQueryHandler : IRequestHandler<GetTripsQuery, Result<PagedResult<TripDto>>>
{
    private readonly ITripRepository _tripRepository;

    public GetTripsQueryHandler(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public async Task<Result<PagedResult<TripDto>>> Handle(GetTripsQuery request, CancellationToken cancellationToken)
    {
        var trips = await _tripRepository.GetPaginatedTripsAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.Status,
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        return Result.Success(trips);
    }
}
