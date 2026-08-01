using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Fleet.Maintenance;

/// <summary>
/// Record of a maintenance event (preventive or corrective) for a vehicle.
/// </summary>
public sealed class MaintenanceRecord : AggregateRoot
{
    private MaintenanceRecord() { } // EF Core

    private MaintenanceRecord(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid vehicleId,
        MaintenanceType type,
        MaintenanceStatus status,
        DateTime date,
        int odometer,
        string description,
        decimal totalCost,
        string? providerName,
        string? invoiceUrl,
        string? notes)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        VehicleId = vehicleId;
        Type = type;
        Status = status;
        Date = date;
        Odometer = odometer;
        Description = description;
        TotalCost = totalCost;
        ProviderName = providerName;
        InvoiceUrl = invoiceUrl;
        Notes = notes;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid VehicleId { get; private set; }
    public MaintenanceType Type { get; private set; }
    public MaintenanceStatus Status { get; private set; }
    
    public DateTime Date { get; private set; }
    public int Odometer { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal TotalCost { get; private set; }
    
    public string? ProviderName { get; private set; }
    public string? InvoiceUrl { get; private set; }
    public string? Notes { get; private set; }

    public static Result<MaintenanceRecord> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid vehicleId,
        MaintenanceType type,
        MaintenanceStatus status,
        DateTime date,
        int odometer,
        string description,
        decimal totalCost,
        string? providerName,
        string? invoiceUrl,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<MaintenanceRecord>(Error.Validation("MaintenanceRecord.DescriptionRequired", "Description is required."));

        if (odometer < 0)
            return Result.Failure<MaintenanceRecord>(Error.Validation("MaintenanceRecord.InvalidOdometer", "Odometer must be non-negative."));

        if (totalCost < 0)
            return Result.Failure<MaintenanceRecord>(Error.Validation("MaintenanceRecord.InvalidCost", "Total cost cannot be negative."));

        var record = new MaintenanceRecord(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            vehicleId, type, status, date, odometer, description.Trim(), totalCost,
            providerName?.Trim(), invoiceUrl?.Trim(), notes?.Trim());

        return Result.Success(record);
    }

    public Result Update(
        MaintenanceType type,
        MaintenanceStatus status,
        DateTime date,
        int odometer,
        string description,
        decimal totalCost,
        string? providerName,
        string? invoiceUrl,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("MaintenanceRecord.DescriptionRequired", "Description is required."));

        if (odometer < 0)
            return Result.Failure(Error.Validation("MaintenanceRecord.InvalidOdometer", "Odometer must be non-negative."));

        if (totalCost < 0)
            return Result.Failure(Error.Validation("MaintenanceRecord.InvalidCost", "Total cost cannot be negative."));

        Type = type;
        Status = status;
        Date = date;
        Odometer = odometer;
        Description = description.Trim();
        TotalCost = totalCost;
        ProviderName = providerName?.Trim();
        InvoiceUrl = invoiceUrl?.Trim();
        Notes = notes?.Trim();
        
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void Delete()
    {
        SoftDelete(Guid.Empty);
    }
}
