using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.InvoiceId)
            .IsRequired();

        builder.Property(x => x.Position)
            .IsRequired();

        builder.Property(x => x.ProductId);

        builder.Property(x => x.ProductCode)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => ProductCode.CreateOptional(value))
            .HasMaxLength(ProductCode.MaxLength)
            .HasColumnName("ProductCode");

        builder.Property(x => x.Description)
            .HasConversion(
                value => value.Value,
                value => InvoiceLineDescription.Create(value))
            .HasMaxLength(InvoiceLineDescription.MaxLength)
            .HasColumnName("Description")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(
                value => value.Value,
                value => Quantity.Create(value))
            .HasPrecision(18, 6)
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(x => x.UnitOfMeasure)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DiscountRate)
            .HasConversion(
                value => value.Value,
                value => Percentage.Create(value))
            .HasPrecision(9, 4)
            .HasColumnName("DiscountRate")
            .IsRequired();

        builder.Property(x => x.TaxRate)
            .HasConversion(
                value => value.Value,
                value => Percentage.Create(value))
            .HasPrecision(9, 4)
            .HasColumnName("TaxRate")
            .IsRequired();

        builder.OwnsOne(x => x.UnitPrice,
            money => ConfigureMoney(money, "UnitPrice"));

        builder.OwnsOne(x => x.DiscountAmount,
            money => ConfigureMoney(money, "Discount"));

        builder.OwnsOne(x => x.NetAmount,
            money => ConfigureMoney(money, "Net"));

        builder.OwnsOne(x => x.TaxAmount,
            money => ConfigureMoney(money, "Tax"));

        builder.OwnsOne(x => x.GrossAmount,
            money => ConfigureMoney(money, "Gross"));

        builder.Navigation(x => x.UnitPrice)
            .IsRequired();

        builder.Navigation(x => x.DiscountAmount)
            .IsRequired();

        builder.Navigation(x => x.NetAmount)
            .IsRequired();

        builder.Navigation(x => x.TaxAmount)
            .IsRequired();

        builder.Navigation(x => x.GrossAmount)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.InvoiceId, x.Position })
            .IsUnique()
            .HasDatabaseName("UX_InvoiceLines_TenantId_InvoiceId_Position");
    }

    private static void ConfigureMoney<TOwner>(OwnedNavigationBuilder<TOwner, Money> builder, string prefix)
        where TOwner : class
    {
        builder.Property(x => x.Amount)
            .HasPrecision(19, 4)
            .HasColumnName($"{prefix}Amount")
            .IsRequired();
        builder.Property(x => x.CurrencyCode)
            .HasConversion(value => value.Value, value => CurrencyCode.Create(value))
            .HasMaxLength(CurrencyCode.MaxLength)
            .HasColumnName($"{prefix}CurrencyCode")
            .IsRequired();
    }
}
