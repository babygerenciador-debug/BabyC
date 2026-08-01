using FleetOS.Domain.Fleet.Fuel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class FuelLogConfiguration : IEntityTypeConfiguration<FuelLog>
{
    public void Configure(EntityTypeBuilder<FuelLog> builder)
    {
        builder.ToTable("FuelLogs");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.TenantId).IsRequired();
        builder.Property(f => f.OrganizationId).IsRequired();
        builder.Property(f => f.BusinessUnitId).IsRequired();
        builder.Property(f => f.VehicleId).IsRequired();

        builder.Property(f => f.Liters)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.AverageConsumption)
            .HasPrecision(18, 2);

        builder.Property(f => f.ReceiptUrl)
            .HasMaxLength(2000);

        builder.Property(f => f.Notes)
            .HasMaxLength(1000);

        builder.HasOne<FleetOS.Domain.Fleet.Vehicles.Vehicle>()
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FleetOS.Domain.Operations.Drivers.Driver>()
            .WithMany()
            .HasForeignKey(f => f.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
