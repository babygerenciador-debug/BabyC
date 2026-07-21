using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

public sealed record GetMyActiveTripQuery : IRequest<Result<TripDto?>>;
