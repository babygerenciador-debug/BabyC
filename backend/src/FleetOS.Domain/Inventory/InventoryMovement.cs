using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Inventory;

public sealed class InventoryMovement : Entity
{
    private InventoryMovement() { }

    private InventoryMovement(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid productId,
        MovementType type,
        LocationType? fromLocationType,
        Guid? fromVehicleId,
        LocationType? toLocationType,
        Guid? toVehicleId,
        int quantity,
        DateTime date,
        string? notes,
        Guid? referenceId)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        ProductId = productId;
        Type = type;
        FromLocationType = fromLocationType;
        FromVehicleId = fromVehicleId;
        ToLocationType = toLocationType;
        ToVehicleId = toVehicleId;
        Quantity = quantity;
        Date = date;
        Notes = notes;
        ReferenceId = referenceId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    
    public LocationType? FromLocationType { get; private set; }
    public Guid? FromVehicleId { get; private set; }
    
    public LocationType? ToLocationType { get; private set; }
    public Guid? ToVehicleId { get; private set; }
    
    public int Quantity { get; private set; }
    public DateTime Date { get; private set; }
    
    public string? Notes { get; private set; }
    
    // Can be used to link this movement to a MaintenanceRecord or PurchaseOrder
    public Guid? ReferenceId { get; private set; }

    public static Result<InventoryMovement> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid productId,
        MovementType type,
        LocationType? fromLocationType,
        Guid? fromVehicleId,
        LocationType? toLocationType,
        Guid? toVehicleId,
        int quantity,
        DateTime date,
        string? notes,
        Guid? referenceId = null)
    {
        if (quantity <= 0)
            return Result.Failure<InventoryMovement>(Error.Validation("InventoryMovement.InvalidQuantity", "Quantity must be greater than zero."));

        if (type == MovementType.Transfer)
        {
            if (fromLocationType == null || toLocationType == null)
                return Result.Failure<InventoryMovement>(Error.Validation("InventoryMovement.LocationsRequired", "Transfer requires both source and destination locations."));
        }

        var movement = new InventoryMovement(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            productId, type, fromLocationType, fromVehicleId, toLocationType, toVehicleId,
            quantity, date, notes?.Trim(), referenceId);

        return Result.Success(movement);
    }
}
