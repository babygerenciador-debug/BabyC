namespace FleetOS.Application.Operations.Checklists;

public sealed record ChecklistItemDto(
    Guid Id,
    string Title,
    bool IsActive,
    int SortOrder);

public sealed record DailyChecklistItemDto(
    Guid Id,
    Guid ChecklistItemId,
    string Title,
    bool IsCompleted,
    DateTimeOffset? CompletedAt);

public sealed record DailyChecklistDto(
    Guid Id,
    Guid VehicleId,
    Guid DriverId,
    string Date,
    string Status,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<DailyChecklistItemDto> Items);

public sealed record ChecklistReportRowDto(
    string Date,
    string VehicleLicensePlate,
    string DriverName,
    string Status,
    int TotalItems,
    int CompletedItems);
