using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId).IsRequired();

        builder.Property(x => x.Id).IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => RoleName.Create(value))
            .HasMaxLength(RoleName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => RoleDescription.CreateOptional(value))
            .HasMaxLength(RoleDescription.MaxLength)
            .HasColumnName("Description");

        builder.Property(x => x.Permissions)
            .HasConversion(
                permissions => permissions.Value,
                value => RolePermissions.Create(value))
            .HasColumnName("Permissions")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex("TenantId", "Name")
            .HasDatabaseName("UX_Roles_TenantId_Name")
            .IsUnique();

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => new { x.TenantId, x.RoleId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
