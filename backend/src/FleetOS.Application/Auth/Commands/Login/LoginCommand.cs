using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Identifier, // Email for Admins/Managers, CPF for Drivers
    string Password,
    string? TenantSlug // Optional for email, required for CPF
) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserDto User,
    IReadOnlyList<FuelAlertDto>? FuelAlert
);

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    Guid TenantId,
    Guid OrganizationId,
    Guid BusinessUnitId,
    string Theme,
    string Language,
    bool IsDriverAccount,
    string? CpfLast4
);

public sealed record FuelAlertDto(
    Guid VehicleId,
    string VehiclePlate,
    string VehicleNickname,
    int DaysSinceLastFuel,
    string AlertMode
);
