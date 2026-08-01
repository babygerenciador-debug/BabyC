using FleetOS.Domain.Fleet.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        // ── Core identity ─────────────────────────────────────────────
        builder.Property(v => v.LicensePlate)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(v => v.Chassi)
            .HasMaxLength(50);

        builder.Property(v => v.Nickname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Brand)
            .HasMaxLength(80);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .HasMaxLength(100);

        builder.Property(v => v.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.AssignedDriverId);

        // ── Documentation ─────────────────────────────────────────────
        builder.Property(v => v.Renavam)
            .HasMaxLength(20);

        builder.Property(v => v.AnttNumber)
            .HasMaxLength(50);

        builder.Property(v => v.AnttExpiry);
        builder.Property(v => v.ArtespExpiry);
        builder.Property(v => v.InsuranceExpiry);
        builder.Property(v => v.LicensingExpiry);

        // ── Fuel Tracking ─────────────────────────────────────────────
        builder.Property(v => v.FuelAlertMode)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(v => v.FuelAlertDays);
        builder.Property(v => v.LastFuelAt);

        builder.Property(v => v.CurrentOdometerKm)
            .HasColumnType("decimal(12,2)");

        // ── Indexes ───────────────────────────────────────────────────
        builder.HasIndex(v => new { v.TenantId, v.LicensePlate })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(v => new { v.TenantId, v.Chassi })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(v => new { v.TenantId, v.Status });

        // ── Relationships ─────────────────────────────────────────────
        builder.HasMany(v => v.Documents)
            .WithOne()
            .HasForeignKey(d => d.VehicleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
