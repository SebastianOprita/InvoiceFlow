using InvoiceFlow.Products.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Products.Infrastructure;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => new { x.TenantId, x.Id });
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasConversion(
                code => code.Value,
                value => ProductCode.Create(value))
            .HasMaxLength(ProductCode.MaxLength)
            .HasColumnName("ProductCode")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => ProductName.Create(value))
            .HasMaxLength(ProductName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => ProductDescription.CreateOptional(value))
            .HasMaxLength(ProductDescription.MaxLength)
            .HasColumnName("Description");

        builder.Property(x => x.UnitOfMeasure)
            .HasConversion<int>()
            .HasColumnName("UnitOfMeasure")
            .IsRequired();

        builder.OwnsOne(x => x.UnitPrice, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnType("decimal(18,3)")
                .HasColumnName("UnitPriceAmount")
                .IsRequired();

            money.Property(x => x.CurrencyCode)
                .HasConversion(
                    currencyCode => currencyCode.Value,
                    value => CurrencyCode.Create(value))
                .HasMaxLength(CurrencyCode.MaxLength)
                .HasColumnName("UnitPriceCurrencyCode")
                .IsRequired();
        });

        builder.Navigation(x => x.UnitPrice)
            .IsRequired();

        builder.Property(x => x.TaxRate)
            .HasConversion(
                taxRate => taxRate.Value,
                value => Percentage.Create(value))
            .HasColumnType("decimal(9,4)")
            .HasColumnName("TaxRate")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .HasDatabaseName("UX_Products_Tenant_ProductCode")
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("IX_Products_Tenant_IsActive");
    }
}
