using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Queries;

public sealed record GetVehiclesQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? Status) : IRequest<Result<PagedResult<VehicleDto>>>;
