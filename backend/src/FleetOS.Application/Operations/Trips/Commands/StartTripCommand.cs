using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record StartTripCommand(Guid Id) : IRequest<Result>;
