using FleetOS.Domain.Operations.Checklists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("ChecklistItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.OrganizationId).IsRequired();
        builder.Property(x => x.BusinessUnitId).IsRequired();

        builder.Property(x => x.Title).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
    }
}

internal sealed class DailyChecklistConfiguration : IEntityTypeConfiguration<DailyChecklist>
{
    public void Configure(EntityTypeBuilder<DailyChecklist> builder)
    {
        builder.ToTable("DailyChecklists");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.OrganizationId).IsRequired();
        builder.Property(x => x.BusinessUnitId).IsRequired();

        builder.Property(x => x.VehicleId).IsRequired();
        builder.Property(x => x.DriverId).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.DailyChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).AutoInclude();

        builder.HasIndex(x => new { x.VehicleId, x.Date }).IsUnique();
    }
}

internal sealed class DailyChecklistItemConfiguration : IEntityTypeConfiguration<DailyChecklistItem>
{
    public void Configure(EntityTypeBuilder<DailyChecklistItem> builder)
    {
        builder.ToTable("DailyChecklistItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DailyChecklistId).IsRequired();
        builder.Property(x => x.ChecklistItemId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsCompleted).IsRequired();
    }
}
