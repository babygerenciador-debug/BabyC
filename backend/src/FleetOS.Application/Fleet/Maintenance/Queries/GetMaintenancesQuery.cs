using FleetOS.Domain.Fleet.Maintenance;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Queries;

public sealed record GetMaintenancesQuery(
    int Page,
    int PageSize,
    Guid? VehicleId,
    MaintenanceType? Type,
    MaintenanceStatus? Status) : IRequest<Result<PagedResult<MaintenanceDto>>>;
