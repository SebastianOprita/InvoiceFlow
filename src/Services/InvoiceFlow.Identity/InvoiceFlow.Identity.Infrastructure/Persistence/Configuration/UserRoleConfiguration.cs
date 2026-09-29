using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(x => new { x.TenantId, x.UserId, x.RoleId });

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.RoleId).IsRequired();
        builder.Property(x => x.AssignedAtUtc).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.RoleId })
            .HasDatabaseName("IX_UserRoles_TenantId_RoleId");

        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("IX_UserRoles_TenantId_UserId");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => new { x.TenantId, x.UserId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => new { x.TenantId, x.RoleId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
