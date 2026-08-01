using FleetOS.Application.Common.Interfaces;
using FleetOS.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FleetOS.Infrastructure.Services;

public sealed class FleetNotificationService : IFleetNotificationService
{
    private readonly IHubContext<FleetHub> _hubContext;

    public FleetNotificationService(IHubContext<FleetHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyDashboardUpdateAsync(CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("DashboardUpdate", cancellationToken);

    public async Task NotifyTripCreatedAsync(Guid tripId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("TripCreated", tripId, cancellationToken);

    public async Task NotifyTripUpdatedAsync(Guid tripId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("TripUpdated", tripId, cancellationToken);

    public async Task NotifyTripVehicleSwappedAsync(Guid tripId, Guid newVehicleId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("TripVehicleSwapped", tripId, newVehicleId, cancellationToken);

    public async Task NotifyVehicleCreatedAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("VehicleCreated", vehicleId, cancellationToken);

    public async Task NotifyVehicleUpdatedAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("VehicleUpdated", vehicleId, cancellationToken);

    public async Task NotifyDriverCreatedAsync(Guid driverId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("DriverCreated", driverId, cancellationToken);

    public async Task NotifyDriverUpdatedAsync(Guid driverId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("DriverUpdated", driverId, cancellationToken);

    public async Task NotifyTransactionCreatedAsync(Guid transactionId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("TransactionCreated", transactionId, cancellationToken);

    public async Task NotifyTransactionUpdatedAsync(Guid transactionId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("TransactionUpdated", transactionId, cancellationToken);

    public async Task NotifyMaintenanceCreatedAsync(Guid maintenanceId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("MaintenanceCreated", maintenanceId, cancellationToken);

    public async Task NotifyMaintenanceUpdatedAsync(Guid maintenanceId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("MaintenanceUpdated", maintenanceId, cancellationToken);

    public async Task NotifyFuelLogCreatedAsync(Guid fuelLogId, CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("FuelLogCreated", fuelLogId, cancellationToken);

    public async Task NotifyStockUpdatedAsync(CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("StockUpdated", cancellationToken);

    public async Task NotifyNotificationCreatedAsync(CancellationToken cancellationToken = default)
        => await _hubContext.Clients.All.SendAsync("ReceiveNotification", cancellationToken);
}
