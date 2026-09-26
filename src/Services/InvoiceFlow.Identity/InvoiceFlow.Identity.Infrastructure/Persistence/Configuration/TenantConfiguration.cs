using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => TenantName.Create(value))
            .HasMaxLength(TenantName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasConversion(
                slug => slug.Value,
                value => TenantSlug.Create(value))
            .HasMaxLength(TenantSlug.MaxLength)
            .HasColumnName("Slug")
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex("Name")
            .HasDatabaseName("UX_Tenants_Name");

        builder.HasIndex("Slug")
            .HasDatabaseName("UX_Tenants_Slug")
            .IsUnique();
    }
}
