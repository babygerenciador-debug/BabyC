namespace FleetOS.Application.Common.Interfaces;

public interface IFleetNotificationService
{
    // Dashboard
    Task NotifyDashboardUpdateAsync(CancellationToken cancellationToken = default);
    
    // Trips
    Task NotifyTripCreatedAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task NotifyTripUpdatedAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task NotifyTripVehicleSwappedAsync(Guid tripId, Guid newVehicleId, CancellationToken cancellationToken = default);
    
    // Vehicles
    Task NotifyVehicleCreatedAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task NotifyVehicleUpdatedAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    
    // Drivers
    Task NotifyDriverCreatedAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task NotifyDriverUpdatedAsync(Guid driverId, CancellationToken cancellationToken = default);
    
    // Finance
    Task NotifyTransactionCreatedAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task NotifyTransactionUpdatedAsync(Guid transactionId, CancellationToken cancellationToken = default);
    
    // Maintenance
    Task NotifyMaintenanceCreatedAsync(Guid maintenanceId, CancellationToken cancellationToken = default);
    Task NotifyMaintenanceUpdatedAsync(Guid maintenanceId, CancellationToken cancellationToken = default);
    
    // Fuel
    Task NotifyFuelLogCreatedAsync(Guid fuelLogId, CancellationToken cancellationToken = default);
    
    // Inventory
    Task NotifyStockUpdatedAsync(CancellationToken cancellationToken = default);
    
    // Notifications
    Task NotifyNotificationCreatedAsync(CancellationToken cancellationToken = default);
}
