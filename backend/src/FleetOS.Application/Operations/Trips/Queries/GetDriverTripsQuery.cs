using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

public sealed record GetDriverTripsQuery() : IRequest<Result<PagedResult<TripDto>>>;
