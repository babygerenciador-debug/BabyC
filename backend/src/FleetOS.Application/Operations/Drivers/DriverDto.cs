namespace FleetOS.Application.Operations.Drivers;

public record DriverDto(
    Guid     Id,
    Guid     UserId,
    string   Name,
    string   Email,
    string   CpfLast4,
    string   CnhNumber,
    string   CnhCategory,
    DateTime CnhExpirationDate,
    bool     IsCnhExpired,
    string   Status,
    string?  Phone,
    string?  PhotoUrl,
    bool     IsAvailable,
    string?  AssignedVehicle,
    DateTimeOffset CreatedAt);
