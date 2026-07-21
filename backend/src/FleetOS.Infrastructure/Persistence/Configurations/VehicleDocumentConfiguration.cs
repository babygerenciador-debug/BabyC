using FleetOS.Domain.Fleet.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("vehicle_documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.ExpirationDate)
            .IsRequired();

        builder.Property(d => d.FileUrl)
            .HasMaxLength(500)
            .IsRequired();
    }
}
