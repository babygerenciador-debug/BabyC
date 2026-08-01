using FleetOS.Domain.Fleet.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("Maintenances");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired();
        builder.Property(m => m.OrganizationId).IsRequired();
        builder.Property(m => m.BusinessUnitId).IsRequired();
        builder.Property(m => m.VehicleId).IsRequired();

        builder.Property(m => m.Type)
            .IsRequired();
            
        builder.Property(m => m.Status)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(m => m.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(m => m.ProviderName)
            .HasMaxLength(255);

        builder.Property(m => m.InvoiceUrl)
            .HasMaxLength(2000);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.HasOne<FleetOS.Domain.Fleet.Vehicles.Vehicle>()
            .WithMany()
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
