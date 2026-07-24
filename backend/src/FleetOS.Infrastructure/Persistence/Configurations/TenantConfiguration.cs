using FleetOS.Domain.Core.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(20);

        builder.Property(t => t.LogoUrl).HasMaxLength(500);
        builder.Property(t => t.PrimaryColor).HasMaxLength(10);
        builder.Property(t => t.TimeZone).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Language).IsRequired().HasMaxLength(10);
        builder.Property(t => t.OwnerSalary).HasDefaultValue(0).HasColumnType("decimal(18,2)");

        builder.HasMany(t => t.Organizations)
               .WithOne()
               .HasForeignKey(o => o.TenantId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
