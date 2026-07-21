using FleetOS.Domain.Operations.Drivers;
using FleetOS.Domain.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.CnhNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.CnhCategory)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(d => d.CnhExpirationDate)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.Phone)
            .HasMaxLength(30);

        builder.Property(d => d.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(d => d.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key to User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Unique CNH within Tenant
        builder.HasIndex(d => new { d.TenantId, d.CnhNumber })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(d => new { d.TenantId, d.Status });
    }
}
