using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

public sealed record UpdateVehicleCommand(
    Guid    Id,
    string  Nickname,
    string? DriverCpf,
    Guid?   DriverId,
    string? Chassi,
    string? Brand,
    string? Color,
    string? Renavam,
    string? AnttNumber,
    int?    Capacity,
    int?    Year,
    string? Model,
    string? PhotoUrl,
    // Document expiries
    DateTime? AnttExpiry,
    DateTime? ArtespExpiry,
    DateTime? InsuranceExpiry,
    DateTime? LicensingExpiry,
    // Fuel alert
    string? FuelAlertMode,
    int?    FuelAlertDays,
    // Status
    string? Status
) : IRequest<Result>;
