using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class PaymentNumberSequenceConfiguration
    : IEntityTypeConfiguration<PaymentNumberSequence>
{
    public void Configure(
        EntityTypeBuilder<PaymentNumberSequence> builder)
    {
        builder.ToTable("PaymentNumberSequences");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.TenantId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Year)
            .HasConversion(
                year => year.Value,
                value => FiscalYear.Create(value))
            .HasColumnName("Year")
            .IsRequired();

        builder.Property(x => x.NextNumber)
            .HasConversion(
                number => number.Value,
                value => SequenceNumber.Create(value))
            .HasColumnName("NextNumber")
            .IsRequired();

        builder.Property(x => x.Prefix)
            .HasConversion(
                prefix => prefix.Value,
                value => PaymentNumberPrefix.Create(value))
            .HasMaxLength(20)
            .HasColumnName("Prefix")
            .IsRequired();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Year
        })
        .HasDatabaseName("UX_PaymentNumberSequences_TenantId_Year")
        .IsUnique();
    }
}
