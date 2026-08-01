namespace FleetOS.Application.Fleet.Fuel;

public sealed record FuelLogDto(
    Guid Id,
    Guid VehicleId,
    string VehicleLicensePlate,
    Guid? DriverId,
    string? DriverName,
    DateTime Date,
    int Odometer,
    decimal Liters,
    decimal TotalCost,
    decimal? AverageConsumption,
    string? ReceiptUrl,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
