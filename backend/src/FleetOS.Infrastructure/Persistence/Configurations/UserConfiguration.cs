using FleetOS.Domain.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOS.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name).IsRequired().HasMaxLength(150);
        builder.Property(u => u.EmailAddress).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        
        // Email must be unique per Tenant (ignoring soft-deleted)
        builder.HasIndex(u => new { u.TenantId, u.EmailAddress }).IsUnique().HasFilter("deleted_at IS NULL");

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.CpfHash).HasMaxLength(256);
        builder.Property(u => u.CpfLast4).HasMaxLength(4);

        builder.Property(u => u.Language).IsRequired().HasMaxLength(10);
        builder.Property(u => u.Theme).IsRequired().HasMaxLength(20);

        builder.HasMany(u => u.RefreshTokens)
               .WithOne()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token).IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.Token).IsUnique();

        builder.Property(r => r.ReplacedByToken).HasMaxLength(256);
    }
}
