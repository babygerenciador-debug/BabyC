using FleetOS.Domain.Inventory;

namespace FleetOS.Application.Inventory;

public sealed record ProductCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? SKU,
    string? Description,
    decimal AverageUnitPrice,
    int AvailableQuantity,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record StockBalanceDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    LocationType LocationType,
    Guid? VehicleId,
    string? VehicleLicensePlate,
    int Quantity,
    int MinimumStockLevel,
    bool IsBelowMinimum,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record InventoryMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    MovementType Type,
    LocationType? FromLocationType,
    Guid? FromVehicleId,
    string? FromVehicleName,
    LocationType? ToLocationType,
    Guid? ToVehicleId,
    string? ToVehicleName,
    int Quantity,
    DateTime Date,
    string? Notes,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt);
