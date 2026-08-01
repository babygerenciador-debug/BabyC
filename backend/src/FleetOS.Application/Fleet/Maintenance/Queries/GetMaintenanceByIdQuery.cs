using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Queries;

public sealed record GetMaintenanceByIdQuery(Guid Id) : IRequest<Result<MaintenanceDto>>;
