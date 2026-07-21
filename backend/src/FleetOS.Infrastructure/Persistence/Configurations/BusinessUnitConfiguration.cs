using FleetOS.Domain.Core.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

public sealed class BusinessUnitConfiguration : IEntityTypeConfiguration<BusinessUnit>
{
    public void Configure(EntityTypeBuilder<BusinessUnit> builder)
    {
        builder.ToTable("business_units");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(20);
        
        // Code must be unique within an Organization
        builder.HasIndex(b => new { b.OrganizationId, b.Code }).IsUnique().HasFilter("deleted_at IS NULL");

        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(b => b.ZipCode).HasMaxLength(10);
        builder.Property(b => b.Street).HasMaxLength(150);
        builder.Property(b => b.Number).HasMaxLength(20);
        builder.Property(b => b.District).HasMaxLength(100);
        builder.Property(b => b.City).HasMaxLength(100);
        builder.Property(b => b.State).HasMaxLength(2);
        
        builder.Property(b => b.Phone).HasMaxLength(20);
        builder.Property(b => b.Email).HasMaxLength(256);
        builder.Property(b => b.TimeZone).IsRequired().HasMaxLength(50);
    }
}
