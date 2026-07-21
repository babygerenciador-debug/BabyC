using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

public sealed record DeleteVehicleCommand(Guid Id) : IRequest<Result>;
