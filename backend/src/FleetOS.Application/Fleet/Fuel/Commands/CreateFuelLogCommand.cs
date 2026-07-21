using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

public sealed record CreateFuelLogCommand(
    Guid VehicleId,
    Guid? DriverId,
    DateTime Date,
    int Odometer,
    decimal Liters,
    decimal TotalCost,
    string? ReceiptUrl,
    string? Notes) : IRequest<Result<Guid>>;
