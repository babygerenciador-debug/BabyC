using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Queries;

public sealed record GetFuelLogsQuery(
    int Page,
    int PageSize,
    Guid? VehicleId,
    Guid? DriverId,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result<PagedResult<FuelLogDto>>>;
