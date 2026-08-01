using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

public sealed record DeleteFuelLogCommand(Guid Id) : IRequest<Result>;
