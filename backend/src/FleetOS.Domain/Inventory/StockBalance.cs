using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Inventory;

public sealed class StockBalance : AggregateRoot
{
    private StockBalance() { }

    private StockBalance(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid productId,
        LocationType locationType,
        Guid? vehicleId,
        int quantity,
        int minimumStockLevel)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        ProductId = productId;
        LocationType = locationType;
        VehicleId = vehicleId;
        Quantity = quantity;
        MinimumStockLevel = minimumStockLevel;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ProductId { get; private set; }
    public LocationType LocationType { get; private set; }
    public Guid? VehicleId { get; private set; }
    
    public int Quantity { get; private set; }
    public int MinimumStockLevel { get; private set; }

    public bool IsBelowMinimum => Quantity <= MinimumStockLevel;

    public static Result<StockBalance> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid productId,
        LocationType locationType,
        Guid? vehicleId,
        int initialQuantity,
        int minimumStockLevel)
    {
        if (locationType == LocationType.Vehicle && !vehicleId.HasValue)
            return Result.Failure<StockBalance>(Error.Validation("StockBalance.VehicleRequired", "Vehicle ID is required for vehicle stock."));

        if (locationType == LocationType.Main && vehicleId.HasValue)
            return Result.Failure<StockBalance>(Error.Validation("StockBalance.VehicleNotAllowed", "Main stock cannot have a vehicle ID."));

        var balance = new StockBalance(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            productId, locationType, vehicleId, initialQuantity, minimumStockLevel);

        return Result.Success(balance);
    }

    public void AddQuantity(int amount)
    {
        if (amount > 0)
        {
            Quantity += amount;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public Result RemoveQuantity(int amount)
    {
        if (amount <= 0)
            return Result.Failure(Error.Validation("StockBalance.InvalidAmount", "Amount must be greater than zero."));

        if (Quantity < amount)
            return Result.Failure(Error.Validation("StockBalance.InsufficientStock", "Insufficient stock."));

        Quantity -= amount;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void UpdateMinimumStockLevel(int newLevel)
    {
        if (newLevel >= 0)
        {
            MinimumStockLevel = newLevel;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
