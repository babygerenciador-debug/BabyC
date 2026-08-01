using FleetOS.Domain.Inventory;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Inventory.Commands;

public sealed record ReceiveStockCommand(
    Guid ProductId,
    LocationType LocationType,
    Guid? VehicleId,
    int Quantity,
    decimal UnitPrice,
    DateTime Date,
    string? Notes,
    Guid? ReferenceId) : IRequest<Result<Guid>>;

public sealed record ConsumeStockCommand(
    Guid ProductId,
    LocationType LocationType,
    Guid? VehicleId,
    int Quantity,
    DateTime Date,
    string? Notes,
    Guid? ReferenceId) : IRequest<Result<Guid>>;

public sealed record TransferStockCommand(
    Guid ProductId,
    Guid? FromVehicleId,
    Guid? ToVehicleId,
    int Quantity,
    DateTime Date,
    string? Notes,
    Guid? ReferenceId) : IRequest<Result<Guid>>;
