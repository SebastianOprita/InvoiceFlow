using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.Property(x => x.InvoiceId)
            .IsRequired();

        builder.Property(x => x.InvoiceNumber)
            .HasConversion(
                value => value.Value,
                value => InvoiceNumber.Create(value))
            .HasMaxLength(InvoiceNumber.MaxLength)
            .HasColumnName("InvoiceNumber")
            .IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(x => x.Amount)
                .HasPrecision(19, 4)
                .HasColumnName("Amount")
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasConversion(
                    value => value.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(CurrencyCode.MaxLength)
                .HasColumnName("CurrencyCode")
                .IsRequired();
        });

        builder.Navigation(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PaymentId })
            .HasDatabaseName("IX_PaymentAllocations_TenantId_PaymentId");

        builder.HasIndex(x => new { x.TenantId, x.InvoiceId })
            .HasDatabaseName("IX_PaymentAllocations_TenantId_InvoiceId");
    }
}
