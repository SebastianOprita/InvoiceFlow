using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoicePaymentSummaryConfiguration
    : IEntityTypeConfiguration<InvoicePaymentSummary>
{
    public void Configure(
        EntityTypeBuilder<InvoicePaymentSummary> builder)
    {
        builder.ToTable("InvoicePaymentSummaries");

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

        builder.OwnsOne(x => x.PaidAmount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("PaidAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasColumnName("PaidCurrencyCode")
                .HasConversion(
                    currency => currency.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(x => x.OutstandingAmount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("OutstandingAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasColumnName("OutstandingCurrencyCode")
                .HasConversion(
                    currency => currency.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PaymentBalanceVersion)
            .IsRequired();

        builder.Property(x => x.SourceUpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.LastAppliedAtUtc)
            .IsRequired();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}
