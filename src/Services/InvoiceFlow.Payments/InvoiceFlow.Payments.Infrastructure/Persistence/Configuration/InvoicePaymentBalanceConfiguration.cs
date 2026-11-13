using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class InvoicePaymentBalanceConfiguration
    : IEntityTypeConfiguration<InvoicePaymentBalance>
{
    public void Configure(EntityTypeBuilder<InvoicePaymentBalance> builder)
    {
        builder.ToTable("InvoicePaymentBalances", table =>
        {
            table.HasCheckConstraint(
                "CK_InvoicePaymentBalances_Amounts",
                """
                [TotalAmount] > 0
                AND [AllocatedAmount] >= 0
                AND [AllocatedAmount] <= [TotalAmount]
                """);

            table.HasCheckConstraint(
                "CK_InvoicePaymentBalances_Version",
                "[Version] >= 0");

            table.HasCheckConstraint(
                "CK_InvoicePaymentBalances_Currency",
                """
                [CurrencyCode] = [TotalCurrencyCode]
                AND [CurrencyCode] = [AllocatedCurrencyCode]
                """);
        });

        // Primary key
        // --------------------------------------------------

        builder.HasKey(x => new
        {
            x.TenantId,
            x.InvoiceId
        });

        builder.Property(x => x.TenantId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.InvoiceId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        // Invoice snapshot
        // --------------------------------------------------

        builder.Property(x => x.InvoiceNumber)
            .HasConversion(
                number => number.Value,
                value => InvoiceNumber.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(
                currency => currency.Value,
                value => CurrencyCode.Create(value))
            .HasMaxLength(3)
            .IsRequired();

        // Total amount
        // --------------------------------------------------

        builder.OwnsOne(x => x.TotalAmount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasColumnName("TotalCurrencyCode")
                .HasConversion(
                    currency => currency.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(x => x.TotalAmount)
            .IsRequired();

        // Allocated amount
        // --------------------------------------------------

        builder.OwnsOne(x => x.AllocatedAmount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("AllocatedAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasColumnName("AllocatedCurrencyCode")
                .HasConversion(
                    currency => currency.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(x => x.AllocatedAmount)
            .IsRequired();

        // Lifecycle
        // --------------------------------------------------

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // Integration event version
        // --------------------------------------------------

        builder.Property(x => x.Version)
            .IsRequired();

        // Timestamps
        // --------------------------------------------------

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        // Optimistic concurrency
        // --------------------------------------------------

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        // Indexes
        // --------------------------------------------------

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CustomerId
        });
    }
}
