using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Fleet.Fuel;

/// <summary>
/// Log of a fueling event for a vehicle.
/// </summary>
public sealed class FuelLog : AggregateRoot
{
    private FuelLog() { } // EF Core

    private FuelLog(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid vehicleId,
        Guid? driverId,
        DateTime date,
        int odometer,
        decimal liters,
        decimal totalCost,
        string? receiptUrl,
        string? notes)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        VehicleId = vehicleId;
        DriverId = driverId;
        Date = date;
        Odometer = odometer;
        Liters = liters;
        TotalCost = totalCost;
        ReceiptUrl = receiptUrl;
        Notes = notes;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid VehicleId { get; private set; }
    public Guid? DriverId { get; private set; }
    
    public DateTime Date { get; private set; }
    public int Odometer { get; private set; }
    
    public decimal Liters { get; private set; }
    public decimal TotalCost { get; private set; }
    
    // Can be calculated if there is a previous log, keeping it nullable
    public decimal? AverageConsumption { get; private set; }
    
    public string? ReceiptUrl { get; private set; }
    public string? Notes { get; private set; }

    public static Result<FuelLog> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid vehicleId,
        Guid? driverId,
        DateTime date,
        int odometer,
        decimal liters,
        decimal totalCost,
        string? receiptUrl,
        string? notes)
    {
        if (odometer < 0)
            return Result.Failure<FuelLog>(Error.Validation("FuelLog.InvalidOdometer", "Odometer must be non-negative."));

        if (liters <= 0)
            return Result.Failure<FuelLog>(Error.Validation("FuelLog.InvalidLiters", "Liters must be greater than zero."));

        if (totalCost <= 0)
            return Result.Failure<FuelLog>(Error.Validation("FuelLog.InvalidCost", "Total cost must be greater than zero."));

        var log = new FuelLog(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            vehicleId, driverId, date, odometer, liters, totalCost,
            receiptUrl?.Trim(), notes?.Trim());

        return Result.Success(log);
    }

    public Result Update(
        Guid? driverId,
        DateTime date,
        int odometer,
        decimal liters,
        decimal totalCost,
        string? receiptUrl,
        string? notes)
    {
        if (odometer < 0)
            return Result.Failure(Error.Validation("FuelLog.InvalidOdometer", "Odometer must be non-negative."));

        if (liters <= 0)
            return Result.Failure(Error.Validation("FuelLog.InvalidLiters", "Liters must be greater than zero."));

        if (totalCost <= 0)
            return Result.Failure(Error.Validation("FuelLog.InvalidCost", "Total cost must be greater than zero."));

        DriverId = driverId;
        Date = date;
        Odometer = odometer;
        Liters = liters;
        TotalCost = totalCost;
        ReceiptUrl = receiptUrl?.Trim();
        Notes = notes?.Trim();
        
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void SetAverageConsumption(decimal consumption)
    {
        if (consumption > 0)
        {
            AverageConsumption = consumption;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Delete()
    {
        // Simplification for MVP. We pass Guid.Empty since we don't have the user context in Domain here.
        SoftDelete(Guid.Empty);
    }
}
