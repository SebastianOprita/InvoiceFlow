using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class ServiceClientSecretConfiguration
    : IEntityTypeConfiguration<ServiceClientSecret>
{
    public void Configure(EntityTypeBuilder<ServiceClientSecret> builder)
    {
        builder.ToTable("ServiceClientSecrets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.ServiceClientId)
            .IsRequired();

        builder.Property(x => x.SecretHash)
            .HasConversion(
                secretHash => secretHash.Value,
                value => ServiceClientSecretHash.Create(value))
            .HasMaxLength(ServiceClientSecretHash.MaxLength)
            .HasColumnName("SecretHash")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => ServiceClientSecretName.Create(value))
            .HasMaxLength(ServiceClientSecretName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => ServiceClientSecretDescription.CreateOptional(value))
            .HasMaxLength(ServiceClientSecretDescription.MaxLength)
            .HasColumnName("Description");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasIndex(x => x.ServiceClientId)
            .HasDatabaseName(
                "IX_ServiceClientSecrets_ServiceClientId");

        builder.HasIndex(x => x.SecretHash)
            .HasDatabaseName(
                "UX_ServiceClientSecrets_SecretHash")
            .IsUnique();
    }
}
