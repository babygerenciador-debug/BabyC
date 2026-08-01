using FleetOS.Domain.Core.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).IsRequired().HasMaxLength(150);
        builder.Property(o => o.Cnpj).HasMaxLength(14);
        
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(o => o.Phone).HasMaxLength(20);
        builder.Property(o => o.Email).HasMaxLength(256);
        builder.Property(o => o.Address).HasMaxLength(250);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.State).HasMaxLength(2);
        builder.Property(o => o.ZipCode).HasMaxLength(10);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);

        builder.HasMany(o => o.BusinessUnits)
               .WithOne()
               .HasForeignKey(bu => bu.OrganizationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
