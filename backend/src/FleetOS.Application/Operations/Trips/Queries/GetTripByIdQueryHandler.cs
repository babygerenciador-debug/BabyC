using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

internal sealed class GetTripByIdQueryHandler : IRequestHandler<GetTripByIdQuery, Result<TripDto>>
{
    private readonly ITripRepository _tripRepository;

    public GetTripByIdQueryHandler(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public async Task<Result<TripDto>> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetTripByIdWithDetailsAsync(request.Id, cancellationToken);
        if (trip is null)
            return Result.Failure<TripDto>(Error.NotFound("Trip.NotFound", "Trip not found."));

        return Result.Success(trip);
    }
}
