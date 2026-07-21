using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

public sealed record GetTripByIdQuery(Guid Id) : IRequest<Result<TripDto>>;
