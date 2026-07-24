using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed record SetDriverAvailabilityCommand(Guid Id, bool IsAvailable) : IRequest<Result>;
