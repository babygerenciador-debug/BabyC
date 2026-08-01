using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record DeleteTripCommand(Guid Id) : IRequest<Result>;
