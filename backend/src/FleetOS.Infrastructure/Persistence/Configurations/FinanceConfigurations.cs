using FleetOS.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

internal sealed class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
    }
}

internal sealed class FinancialCategoryConfiguration : IEntityTypeConfiguration<FinancialCategory>
{
    public void Configure(EntityTypeBuilder<FinancialCategory> builder)
    {
        builder.ToTable("FinancialCategories");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Type).IsRequired();
    }
}

internal sealed class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId).IsRequired();
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.Status).IsRequired();
        
        builder.Property(t => t.Description).HasMaxLength(1000).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();

        builder.HasOne<FinancialCategory>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(t => t.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
