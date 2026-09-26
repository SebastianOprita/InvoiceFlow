using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class PlatformUserConfiguration : IEntityTypeConfiguration<PlatformUser>
{
    public void Configure(EntityTypeBuilder<PlatformUser> builder)
    {
        builder.ToTable("PlatformUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => UserEmail.Create(value))
            .HasMaxLength(UserEmail.MaxLength)
            .HasColumnName("Email")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasConversion(
                passwordHash => passwordHash.Value,
                value => PasswordHash.Create(value))
            .HasMaxLength(512)
            .HasColumnName("PasswordHash")
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasConversion(
                firstName => firstName.Value,
                value => FirstName.Create(value))
            .HasMaxLength(FirstName.MaxLength)
            .HasColumnName("FirstName")
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasConversion(
                lastName => lastName.Value,
                value => LastName.Create(value))
            .HasMaxLength(LastName.MaxLength)
            .HasColumnName("LastName")
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex("Email")
            .HasDatabaseName("UX_PlatformUsers_Email")
            .IsUnique();

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_PlatformUsers_IsActive");
    }
}
