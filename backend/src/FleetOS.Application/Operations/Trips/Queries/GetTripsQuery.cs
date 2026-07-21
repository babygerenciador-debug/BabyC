using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

public sealed record GetTripsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? Status,
    Guid? DriverId,
    Guid? VehicleId) : IRequest<Result<PagedResult<TripDto>>>;
