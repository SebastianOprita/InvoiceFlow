using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoiceNumberSequenceConfiguration
    : IEntityTypeConfiguration<InvoiceNumberSequence>
{
    public void Configure(
        EntityTypeBuilder<InvoiceNumberSequence> builder)
    {
        builder.ToTable("InvoiceNumberSequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Year)
            .HasConversion(
                year => year.Value,
                value => InvoiceYear.Create(value))
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
                value => InvoiceNumberPrefix.Create(value))
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
        .HasDatabaseName("UX_InvoiceNumberSequences_TenantId_Year")
        .IsUnique();
    }
}
