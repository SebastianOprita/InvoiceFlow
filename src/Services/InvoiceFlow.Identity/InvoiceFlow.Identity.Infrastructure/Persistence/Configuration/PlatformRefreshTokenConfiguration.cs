using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class PlatformRefreshTokenConfiguration : IEntityTypeConfiguration<PlatformRefreshToken>
{
    public void Configure(EntityTypeBuilder<PlatformRefreshToken> builder)
    {
        builder.ToTable("PlatformRefreshTokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.TokenHash)
            .HasConversion(
                tokenHash => tokenHash.Value,
                value => RefreshTokenHash.Create(value))
            .HasMaxLength(RefreshTokenHash.MaxLength)
            .HasColumnName("TokenHash")
            .IsRequired();

        builder.Property(x => x.DeviceInfo)
            .HasConversion(
                deviceInfo => deviceInfo == null ? null : deviceInfo.Value,
                value => DeviceInfo.CreateOptional(value))
            .HasMaxLength(DeviceInfo.MaxLength)
            .HasColumnName("DeviceInfo");

        builder.Property(x => x.IpAddress)
            .HasConversion(
                ipAddress => ipAddress == null ? null : ipAddress.Value,
                value => IpAddress.CreateOptional(value))
            .HasMaxLength(IpAddress.MaxLength)
            .HasColumnName("IpAddress");

        builder.Property(x => x.ReplacedByTokenId);
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.RevokedAtUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_PlatformRefreshTokens_UserId");

        builder.HasIndex("TokenHash")
            .HasDatabaseName("UX_PlatformRefreshTokens_TokenHash")
            .IsUnique();
    }
}
