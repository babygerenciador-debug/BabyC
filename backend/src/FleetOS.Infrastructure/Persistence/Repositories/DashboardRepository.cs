using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Dashboard.Queries;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Finance;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Inventory;
using FleetOS.Domain.Operations.Trips;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class DashboardRepository : IDashboardRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DashboardRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalVehicles = await _dbContext.Set<Vehicle>().CountAsync(v => v.TenantId == tenantId, cancellationToken);
        var availableVehicles = await _dbContext.Set<Vehicle>().CountAsync(v => v.TenantId == tenantId && v.Status == VehicleStatus.Available, cancellationToken);
        var inTripVehicles = await _dbContext.Set<Vehicle>().CountAsync(v => v.TenantId == tenantId && v.Status == VehicleStatus.InTrip, cancellationToken);
        var inMaintenanceVehicles = await _dbContext.Set<Vehicle>().CountAsync(v => v.TenantId == tenantId && v.Status == VehicleStatus.InMaintenance, cancellationToken);

        var totalTripsThisMonth = await _dbContext.Set<Trip>().CountAsync(t => t.TenantId == tenantId && t.CreatedAt >= startOfMonth, cancellationToken);
        var ongoingTrips = await _dbContext.Set<Trip>().CountAsync(t => t.TenantId == tenantId && t.Status == TripStatus.InProgress, cancellationToken);

        var stockAlerts = await _dbContext.Set<StockBalance>().CountAsync(s => s.TenantId == tenantId && s.Quantity <= s.MinimumStockLevel, cancellationToken);

        var monthRevenues = await _dbContext.Set<FinancialTransaction>()
            .Where(t => t.TenantId == tenantId && t.Status == TransactionStatus.Paid && t.Date >= startOfMonth && t.Type == TransactionType.Revenue)
            .SumAsync(t => t.Amount, cancellationToken);

        var monthExpenses = await _dbContext.Set<FinancialTransaction>()
            .Where(t => t.TenantId == tenantId && t.Status == TransactionStatus.Paid && t.Date >= startOfMonth && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount, cancellationToken);

        var fuelExpenses = await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == tenantId && f.Date >= startOfMonth)
            .SumAsync(f => f.TotalCost, cancellationToken);

        monthExpenses += fuelExpenses;
        var monthBalance = monthRevenues - monthExpenses;

        var ownerSalary = await _dbContext.Set<Tenant>()
            .Where(t => t.Id == tenantId)
            .Select(t => t.OwnerSalary)
            .FirstOrDefaultAsync(cancellationToken);

        var ownerTaxRate = 0.27m;
        var netOwnerSalary = ownerSalary * (1 - ownerTaxRate);
        var monthRealProfit = netOwnerSalary + monthBalance;

        return new DashboardSummaryDto(
            totalVehicles,
            availableVehicles,
            inTripVehicles,
            inMaintenanceVehicles,
            totalTripsThisMonth,
            ongoingTrips,
            stockAlerts,
            monthRevenues,
            monthExpenses,
            monthBalance,
            monthRealProfit
        );
    }
}
