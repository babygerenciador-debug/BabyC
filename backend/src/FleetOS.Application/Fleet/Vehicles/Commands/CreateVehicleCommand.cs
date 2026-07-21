using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

public sealed record CreateVehicleCommand(
    string  LicensePlate,
    string  Nickname,
    string? DriverCpf,
    string? Chassi,
    string? Brand,
    string? Color,
    string? Renavam,
    string? AnttNumber,
    int?    Capacity,
    int?    Year,
    string? Model,
    // Document expiries
    DateTime? AnttExpiry,
    DateTime? ArtespExpiry,
    DateTime? InsuranceExpiry,
    DateTime? LicensingExpiry,
    // Fuel alert
    string? FuelAlertMode,
    int?    FuelAlertDays
) : IRequest<Result<Guid>>;
