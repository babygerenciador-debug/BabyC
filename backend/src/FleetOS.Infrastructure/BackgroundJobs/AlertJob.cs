using FleetOS.Domain.Common.Notifications;
using FleetOS.Domain.Inventory;
using FleetOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetOS.Infrastructure.BackgroundJobs;

public class AlertJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlertJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public AlertJob(IServiceProvider serviceProvider, ILogger<AlertJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing AlertJob.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckLowStockAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FleetOsDbContext>();

        var tenants = await dbContext.Set<Domain.Core.Tenants.Tenant>()
            .Where(t => t.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (tenants.Count == 0) return;

        var allNotifications = new List<Notification>();

        foreach (var tenant in tenants)
        {
            dbContext.SetTenantId(tenant.Id);

            var lowStockProducts = await dbContext.Set<Product>()
                .Where(p => p.DeletedAt == null)
                .Select(p => new
                {
                    Product = p,
                    TotalStock = dbContext.Set<StockBalance>()
                        .Where(s => s.ProductId == p.Id && s.DeletedAt == null)
                        .Sum(s => (int?)s.Quantity) ?? 0
                })
                .Where(x => x.TotalStock <= 5)
                .ToListAsync(cancellationToken);

            if (lowStockProducts.Count == 0) continue;

            _logger.LogInformation(
                "Tenant {TenantId}: Found {Count} products with low stock.",
                tenant.Id, lowStockProducts.Count);

            foreach (var item in lowStockProducts)
            {
                var p = item.Product;
                var title = $"Estoque Baixo: {p.Name}";
                var message = $"O produto {p.Name} (SKU: {p.SKU}) possui apenas {item.TotalStock} unidades em estoque.";

                var notification = Notification.Create(
                    p.TenantId,
                    p.OrganizationId,
                    p.BusinessUnitId,
                    null,
                    "SystemAdmin",
                    title,
                    message,
                    NotificationType.Warning);

                if (notification.IsSuccess)
                    allNotifications.Add(notification.Value!);
            }
        }

        if (allNotifications.Any())
        {
            await dbContext.Set<Notification>().AddRangeAsync(allNotifications, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
