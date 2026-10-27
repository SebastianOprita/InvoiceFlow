using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Number)
            .HasConversion(
                number => number == null ? null : number.Value,
                value => InvoiceNumber.CreateOptional(value))
            .HasMaxLength(InvoiceNumber.MaxLength)
            .HasColumnName("Number");

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.CustomerCode)
            .HasConversion(
                value => value.Value,
                value => CustomerCode.Create(value))
            .HasMaxLength(CustomerCode.MaxLength)
            .HasColumnName("CustomerCode")
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasConversion(
                value => value.Value,
                value => CustomerName.Create(value))
            .HasMaxLength(CustomerName.MaxLength)
            .HasColumnName("CustomerName")
            .IsRequired();

        builder.Property(x => x.CustomerEmail)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => CustomerEmail.CreateOptional(value))
            .HasMaxLength(CustomerEmail.MaxLength)
            .HasColumnName("CustomerEmail");

        builder.Property(x => x.CustomerPhone)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => PhoneNumber.CreateOptional(value))
            .HasMaxLength(PhoneNumber.MaxLength)
            .HasColumnName("CustomerPhone");

        builder.Property(x => x.CustomerTaxNumber)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => TaxNumber.CreateOptional(value))
            .HasMaxLength(TaxNumber.MaxLength)
            .HasColumnName("CustomerTaxNumber");

        builder.Property(x => x.CustomerRegistrationNumber)
            .HasConversion(
                value => value.Value,
                value => RegistrationNumber.Create(value))
            .HasMaxLength(RegistrationNumber.MaxLength)
            .HasColumnName("CustomerRegistrationNumber")
            .IsRequired();

        builder.Property(x => x.CustomerAddressLine1)
            .HasConversion(
                value => value.Value,
                value => AddressLine1.Create(value))
            .HasMaxLength(AddressLine1.MaxLength)
            .HasColumnName("CustomerAddressLine1")
            .IsRequired();

        builder.Property(x => x.CustomerAddressLine2)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => AddressLine2.CreateOptional(value))
            .HasMaxLength(AddressLine2.MaxLength)
            .HasColumnName("CustomerAddressLine2");

        builder.Property(x => x.CustomerCity)
            .HasConversion(
                value => value.Value,
                value => City.Create(value))
            .HasMaxLength(City.MaxLength)
            .HasColumnName("CustomerCity")
            .IsRequired();

        builder.Property(x => x.CustomerState)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => State.CreateOptional(value))
            .HasMaxLength(State.MaxLength)
            .HasColumnName("CustomerState");

        builder.Property(x => x.CustomerCountry)
            .HasConversion(
                value => value.Value,
                value => Country.Create(value))
            .HasMaxLength(Country.MaxLength)
            .HasColumnName("CustomerCountry")
            .IsRequired();

        builder.Property(x => x.CustomerPostalCode)
            .HasConversion(
                value => value.Value,
                value => PostalCode.Create(value))
            .HasMaxLength(PostalCode.MaxLength)
            .HasColumnName("CustomerPostalCode")
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(
                value => value.Value,
                value => CurrencyCode.Create(value))
            .HasMaxLength(CurrencyCode.MaxLength)
            .HasColumnName("CurrencyCode")
            .IsRequired();

        builder.Property(x => x.PaymentTermDays)
            .HasConversion(
                value => value.Value,
                value => PaymentTermDays.Create(value))
            .HasColumnName("PaymentTermDays")
            .IsRequired();

        builder.Property(x => x.IssueDate)
            .HasColumnName("IssueDate");

        builder.Property(x => x.DueDate)
            .HasColumnName("DueDate");

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => InvoiceNotes.CreateOptional(value))
            .HasMaxLength(InvoiceNotes.MaxLength)
            .HasColumnName("Notes");

        builder.Property(x => x.IssuedAtUtc);

        builder.Property(x => x.CancelledAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.OwnsOne(x => x.DiscountAmount,
            money => ConfigureMoney(money, "Discount"));

        builder.OwnsOne(x => x.NetAmount,
            money => ConfigureMoney(money, "Net"));

        builder.OwnsOne(x => x.TaxAmount,
            money => ConfigureMoney(money, "Tax"));

        builder.OwnsOne(x => x.GrossAmount,
            money => ConfigureMoney(money, "Gross"));

        builder.OwnsOne(x => x.PaidAmount,
            money => ConfigureMoney(money, "Paid"));

        builder.Navigation(x => x.DiscountAmount)
            .IsRequired();

        builder.Navigation(x => x.NetAmount)
            .IsRequired();

        builder.Navigation(x => x.TaxAmount)
            .IsRequired();

        builder.Navigation(x => x.GrossAmount)
            .IsRequired();

        builder.Navigation(x => x.PaidAmount)
            .IsRequired();

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => new { x.TenantId, x.InvoiceId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        // Payment status is derived from these amount columns. A persisted
        // computed column gives the database an indexable representation.
        builder.Property<int>("PaymentStatusIndex")
            .HasComputedColumnSql(
                "CASE WHEN [PaidAmount] = 0 THEN 1 WHEN [PaidAmount] < [GrossAmount] THEN 2 ELSE 3 END",
                stored: true)
            .HasColumnName("PaymentStatus");

        builder.HasIndex(x => new { x.TenantId, x.Number })
            .IsUnique()
            .HasDatabaseName("UX_Invoices_TenantId_Number")
            .HasFilter("[Number] IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("IX_Invoices_TenantId_Status");

        builder.HasIndex(x => new { x.TenantId, x.CustomerCode })
            .HasDatabaseName("IX_Invoices_TenantId_CustomerCode");

        builder.HasIndex("TenantId", "PaymentStatusIndex")
            .HasDatabaseName("IX_Invoices_TenantId_PaymentStatus");
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
