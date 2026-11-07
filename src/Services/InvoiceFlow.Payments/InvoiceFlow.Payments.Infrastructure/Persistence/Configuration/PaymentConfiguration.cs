using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Number)
            .HasConversion(
                value => value.Value,
                value => PaymentNumber.Create(value))
            .HasMaxLength(PaymentNumber.MaxLength)
            .HasColumnName("Number")
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(
                value => value.Value,
                value => CurrencyCode.Create(value))
            .HasMaxLength(CurrencyCode.MaxLength)
            .HasColumnName("CurrencyCode")
            .IsRequired();

        builder.OwnsOne(x => x.Amount,
            money => ConfigureMoney(money, "Amount"));

        builder.Navigation(x => x.Amount)
            .IsRequired();

        // Calculated domain properties
        builder.Ignore(x => x.AllocatedAmount);
        builder.Ignore(x => x.UnallocatedAmount);

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => PaymentReference.CreateOptional(value))
            .HasMaxLength(PaymentReference.MaxLength)
            .HasColumnName("Reference");

        builder.Property(x => x.PaymentDate)
            .HasColumnName("PaymentDate")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => PaymentNotes.CreateOptional(value))
            .HasMaxLength(PaymentNotes.MaxLength)
            .HasColumnName("Notes");

        builder.Property(x => x.PostedAtUtc);

        builder.Property(x => x.CancelledAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        // Payment allocations
        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => new { x.TenantId, x.PaymentId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Allocations)
            .HasField("_allocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Optimistic concurrency
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.Number })
            .IsUnique()
            .HasDatabaseName("UX_Payments_TenantId_Number");

        builder.HasIndex(x => new { x.TenantId, x.CustomerId })
            .HasDatabaseName("IX_Payments_TenantId_CustomerId");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("IX_Payments_TenantId_Status");

        builder.HasIndex(x => new { x.TenantId, x.PaymentDate })
            .HasDatabaseName("IX_Payments_TenantId_PaymentDate");
    }

    private static void ConfigureMoney<TOwner>(
        OwnedNavigationBuilder<TOwner, Money> builder,
        string prefix)
        where TOwner : class
    {
        builder.Property(x => x.Amount)
            .HasPrecision(19, 4)
            .HasColumnName($"{prefix}Amount")
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(
                value => value.Value,
                value => CurrencyCode.Create(value))
            .HasMaxLength(CurrencyCode.MaxLength)
            .HasColumnName($"{prefix}CurrencyCode")
            .IsRequired();
    }
}
