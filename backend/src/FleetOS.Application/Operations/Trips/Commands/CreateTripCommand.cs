using FleetOS.Domain.Operations.Trips;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record CreateTripCommand(
    Guid DriverId,
    Guid VehicleId,
    string Origin,
    string Destination,
    DateTime ScheduledStartDate,
    DateTime ScheduledEndDate,
    decimal TripValue,
    PaymentStatus PaymentStatus,
    string? Notes) : IRequest<Result<Guid>>;
