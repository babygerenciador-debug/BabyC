using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Domain.Common;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FleetOS.Infrastructure.Persistence;

/// <summary>
/// Main EF Core DbContext for FleetOS.
/// Applies Global Query Filters for multi-tenant isolation and soft delete.
/// </summary>
public sealed class FleetOsDbContext : DbContext, IUnitOfWork
{
    private readonly AuditInterceptor _auditInterceptor;
    private readonly IPublisher _publisher;
    private readonly ILogger<FleetOsDbContext> _logger;
    private Guid _currentTenantId;

    public FleetOsDbContext(
        DbContextOptions<FleetOsDbContext> options,
        AuditInterceptor auditInterceptor,
        IPublisher publisher,
        ILogger<FleetOsDbContext> logger)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
        _publisher = publisher;
        _logger = logger;
    }

    // ─── DbSets ───────────────────────────────────────────────────────

    // Core
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Operations
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<FleetOS.Domain.Operations.Trips.Trip> Trips => Set<FleetOS.Domain.Operations.Trips.Trip>();

    // Fleet
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<FleetOS.Domain.Fleet.Fuel.FuelLog> FuelLogs => Set<FleetOS.Domain.Fleet.Fuel.FuelLog>();
    public DbSet<FleetOS.Domain.Fleet.Maintenance.MaintenanceRecord> Maintenances => Set<FleetOS.Domain.Fleet.Maintenance.MaintenanceRecord>();

    // Inventory
    public DbSet<FleetOS.Domain.Inventory.ProductCategory> ProductCategories => Set<FleetOS.Domain.Inventory.ProductCategory>();
    public DbSet<FleetOS.Domain.Inventory.Product> Products => Set<FleetOS.Domain.Inventory.Product>();
    public DbSet<FleetOS.Domain.Inventory.StockBalance> StockBalances => Set<FleetOS.Domain.Inventory.StockBalance>();
    public DbSet<FleetOS.Domain.Inventory.InventoryMovement> InventoryMovements => Set<FleetOS.Domain.Inventory.InventoryMovement>();

    // Finance
    public DbSet<FleetOS.Domain.Finance.CostCenter> CostCenters => Set<FleetOS.Domain.Finance.CostCenter>();
    public DbSet<FleetOS.Domain.Finance.FinancialCategory> FinancialCategories => Set<FleetOS.Domain.Finance.FinancialCategory>();
    public DbSet<FleetOS.Domain.Finance.FinancialMonth> FinancialMonths => Set<FleetOS.Domain.Finance.FinancialMonth>();
    public DbSet<FleetOS.Domain.Finance.FinancialTransaction> FinancialTransactions => Set<FleetOS.Domain.Finance.FinancialTransaction>();

    // Notifications & Issues
    public DbSet<FleetOS.Domain.Common.Notifications.Notification> Notifications => Set<FleetOS.Domain.Common.Notifications.Notification>();
    public DbSet<FleetOS.Domain.Fleet.VehicleIssues.VehicleIssueReport> VehicleIssueReports => Set<FleetOS.Domain.Fleet.VehicleIssues.VehicleIssueReport>();

    // Checklists
    public DbSet<FleetOS.Domain.Operations.Checklists.ChecklistItem> ChecklistItems => Set<FleetOS.Domain.Operations.Checklists.ChecklistItem>();
    public DbSet<FleetOS.Domain.Operations.Checklists.DailyChecklist> DailyChecklists => Set<FleetOS.Domain.Operations.Checklists.DailyChecklist>();

    // ─── Configuration ────────────────────────────────────────────────
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetOsDbContext).Assembly);

        // ── Global Query Filters ──────────────────────────────────────
        modelBuilder.Entity<Tenant>()
            .HasQueryFilter(t => t.DeletedAt == null);

        // All multi-tenant entities
        modelBuilder.Entity<Organization>()
            .HasQueryFilter(o => o.DeletedAt == null && o.TenantId == _currentTenantId);

        modelBuilder.Entity<BusinessUnit>()
            .HasQueryFilter(bu => bu.DeletedAt == null && bu.TenantId == _currentTenantId);

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.DeletedAt == null && u.TenantId == _currentTenantId);

        modelBuilder.Entity<Driver>()
            .HasQueryFilter(d => d.DeletedAt == null && d.TenantId == _currentTenantId);

        modelBuilder.Entity<Vehicle>()
            .HasQueryFilter(v => v.DeletedAt == null && v.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Operations.Trips.Trip>()
            .HasQueryFilter(t => t.DeletedAt == null && t.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Fleet.Fuel.FuelLog>()
            .HasQueryFilter(f => f.DeletedAt == null && f.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Fleet.Maintenance.MaintenanceRecord>()
            .HasQueryFilter(m => m.DeletedAt == null && m.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Inventory.ProductCategory>()
            .HasQueryFilter(c => c.DeletedAt == null && c.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Inventory.Product>()
            .HasQueryFilter(p => p.DeletedAt == null && p.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Inventory.StockBalance>()
            .HasQueryFilter(s => s.DeletedAt == null && s.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Inventory.InventoryMovement>()
            .HasQueryFilter(m => m.DeletedAt == null && m.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Finance.CostCenter>()
            .HasQueryFilter(c => c.DeletedAt == null && c.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Finance.FinancialCategory>()
            .HasQueryFilter(c => c.DeletedAt == null && c.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Finance.FinancialMonth>()
            .HasQueryFilter(m => m.DeletedAt == null && m.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Finance.FinancialTransaction>()
            .HasQueryFilter(t => t.DeletedAt == null && t.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Common.Notifications.Notification>()
            .HasQueryFilter(n => n.DeletedAt == null && n.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Fleet.VehicleIssues.VehicleIssueReport>()
            .HasQueryFilter(i => i.DeletedAt == null && i.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Operations.Checklists.ChecklistItem>()
            .HasQueryFilter(c => c.DeletedAt == null && c.TenantId == _currentTenantId);

        modelBuilder.Entity<FleetOS.Domain.Operations.Checklists.DailyChecklist>()
            .HasQueryFilter(c => c.DeletedAt == null && c.TenantId == _currentTenantId);

        base.OnModelCreating(modelBuilder);
    }

    public void SetTenantId(Guid tenantId, Guid? userId = null)
    {
        _currentTenantId = tenantId;
    }

    // ─── Domain Events ────────────────────────────────────────────────
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    // ─── IUnitOfWork Implementation ───────────────────────────────────
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CommitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CommitAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        _currentTenantId = tenantId;
        return await SaveChangesAsync(cancellationToken);
    }
}
