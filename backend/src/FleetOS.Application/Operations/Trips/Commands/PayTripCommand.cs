using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record PayTripCommand(Guid Id) : IRequest<Result>;
