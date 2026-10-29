using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class ServiceClientConfiguration : IEntityTypeConfiguration<ServiceClient>
{
    public void Configure(EntityTypeBuilder<ServiceClient> builder)
    {
        builder.ToTable("ServiceClients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.ClientId)
            .HasConversion(
                clientId => clientId.Value,
                value => ServiceClientId.Create(value))
            .HasMaxLength(ServiceClientId.MaxLength)
            .HasColumnName("ClientId")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => ServiceClientName.Create(value))
            .HasMaxLength(ServiceClientName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CanAccessAllTenants)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClientId)
            .HasDatabaseName("UX_ServiceClients_ClientId")
            .IsUnique();

        builder.HasIndex(x => x.OrganizationId)
            .HasDatabaseName("IX_ServiceClients_OrganizationId");
    }
}
