using FleetOS.Domain.Fleet.Maintenance;

namespace FleetOS.Application.Fleet.Maintenance;

public sealed record MaintenanceDto(
    Guid Id,
    Guid VehicleId,
    string VehicleLicensePlate,
    MaintenanceType Type,
    MaintenanceStatus Status,
    DateTime Date,
    int Odometer,
    string Description,
    decimal TotalCost,
    string? ProviderName,
    string? InvoiceUrl,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
