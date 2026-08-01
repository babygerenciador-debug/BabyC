using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record SwapTripVehicleCommand(Guid TripId, Guid NewVehicleId) : IRequest<Result>;
