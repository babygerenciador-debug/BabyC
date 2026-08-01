namespace FleetOS.Application.Operations.Trips;

public sealed record TripDto(
    Guid Id,
    Guid DriverId,
    string DriverName,
    Guid VehicleId,
    string VehicleLicensePlate,
    string Origin,
    string Destination,
    DateTime ScheduledStartDate,
    DateTime ScheduledEndDate,
    decimal TripValue,
    string PaymentStatus,
    string? Notes,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    bool ChecklistCompleted,
    string? ChecklistNotes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
