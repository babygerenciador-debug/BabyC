using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

public sealed record DeleteMaintenanceCommand(Guid Id) : IRequest<Result>;
