using FleetOS.Domain.Common.Notifications;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetOS.Infrastructure.BackgroundJobs;

public class RefuelReminderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefuelReminderJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public RefuelReminderJob(IServiceProvider serviceProvider, ILogger<RefuelReminderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RefuelReminderJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckRefuelAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing RefuelReminderJob.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckRefuelAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FleetOsDbContext>();

        var tenants = await dbContext.Set<Tenant>()
            .Where(t => t.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (tenants.Count == 0)
        {
            _logger.LogWarning("No active tenants found for RefuelReminderJob.");
            return;
        }

        var thresholdDate = DateTime.UtcNow.AddDays(-10);
        var allNotifications = new List<Notification>();

        foreach (var tenant in tenants)
        {
            dbContext.SetTenantId(tenant.Id);

            var vehiclesNeedingRefuel = await dbContext.Set<Vehicle>()
                .Where(v => v.Status != VehicleStatus.OutOfService)
                .Select(v => new
                {
                    Vehicle = v,
                    LastFuelLogDate = dbContext.Set<FleetOS.Domain.Fleet.Fuel.FuelLog>()
                        .Where(f => f.VehicleId == v.Id)
                        .OrderByDescending(f => f.Date)
                        .Select(f => (DateTime?)f.Date)
                        .FirstOrDefault()
                })
                .Where(x => x.LastFuelLogDate != null && x.LastFuelLogDate < thresholdDate)
                .ToListAsync(cancellationToken);

            if (!vehiclesNeedingRefuel.Any())
                continue;

            _logger.LogInformation(
                "Tenant {TenantId}: Found {Count} vehicles needing refuel.",
                tenant.Id, vehiclesNeedingRefuel.Count);

            foreach (var item in vehiclesNeedingRefuel)
            {
                var v = item.Vehicle;
                var daysSince = (DateTime.UtcNow - item.LastFuelLogDate!.Value).Days;

                var title = $"Lembrete de Abastecimento: {v.LicensePlate}";
                var message = $"O veículo {v.LicensePlate} foi abastecido pela última vez há {daysSince} dias. Considere realizar um novo abastecimento.";

                var adminNotificationResult = Notification.Create(
                    v.TenantId,
                    v.OrganizationId,
                    v.BusinessUnitId,
                    null,
                    "SystemAdmin",
                    title,
                    message,
                    NotificationType.Warning);

                if (adminNotificationResult.IsSuccess)
                    allNotifications.Add(adminNotificationResult.Value!);
            }
        }

        if (allNotifications.Any())
        {
            await dbContext.Set<Notification>().AddRangeAsync(allNotifications, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
