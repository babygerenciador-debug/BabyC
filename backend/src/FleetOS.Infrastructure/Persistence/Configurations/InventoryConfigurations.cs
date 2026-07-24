using FleetOS.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        builder.Property(p => p.SKU).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(2000);
        
        builder.Property(p => p.AverageUnitPrice).HasPrecision(18, 2);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.LocationType).IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.ProductId })
            .IsUnique()
            .HasFilter("\"location_type\" = 1 AND \"vehicle_id\" IS NULL");

        builder.HasIndex(s => new { s.TenantId, s.ProductId, s.VehicleId })
            .IsUnique()
            .HasFilter("\"location_type\" = 2 AND \"vehicle_id\" IS NOT NULL");
        
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FleetOS.Domain.Fleet.Vehicles.Vehicle>()
            .WithMany()
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired();
        builder.Property(m => m.Type).IsRequired();
        
        builder.Property(m => m.Notes).HasMaxLength(1000);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FleetOS.Domain.Fleet.Vehicles.Vehicle>()
            .WithMany()
            .HasForeignKey(m => m.FromVehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne<FleetOS.Domain.Fleet.Vehicles.Vehicle>()
            .WithMany()
            .HasForeignKey(m => m.ToVehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
