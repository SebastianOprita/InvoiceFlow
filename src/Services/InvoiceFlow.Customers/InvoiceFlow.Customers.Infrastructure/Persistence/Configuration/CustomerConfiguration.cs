using InvoiceFlow.Customers.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Customers.Infrastructure;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // Primary Key (Composite)
        builder.HasKey(x => new { x.TenantId, x.Id });

        // Basic Properties
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.CustomerCode)
            .HasConversion(
                customerCode => customerCode.Value,
                value => CustomerCode.Create(value))
            .HasMaxLength(CustomerCode.MaxLength)
            .HasColumnName("CustomerCode")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => CustomerName.Create(value))
            .HasMaxLength(CustomerName.MaxLength)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                email => email == null ? null : email.Value,
                value => CustomerEmail.CreateOptional(value))
            .HasMaxLength(CustomerEmail.MaxLength)
            .HasColumnName("Email");

        builder.Property(x => x.Phone)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => PhoneNumber.CreateOptional(value))
            .HasMaxLength(PhoneNumber.MaxLength)
            .HasColumnName("Phone");

        builder.Property(x => x.TaxNumber)
            .HasConversion(
                taxNumber => taxNumber == null ? null : taxNumber.Value,
                value => TaxNumber.CreateOptional(value))
            .HasMaxLength(TaxNumber.MaxLength)
            .HasColumnName("TaxNumber");

        builder.Property(x => x.RegistrationNumber)
            .HasConversion(
                registrationNumber => registrationNumber.Value,
                value => RegistrationNumber.Create(value))
            .HasMaxLength(RegistrationNumber.MaxLength)
            .HasColumnName("RegistrationNumber")
            .IsRequired();

        builder.Property(x => x.AddressLine1)
            .HasConversion(
                addressLine1 => addressLine1.Value,
                value => AddressLine1.Create(value))
            .HasMaxLength(AddressLine1.MaxLength)
            .HasColumnName("AddressLine1")
            .IsRequired();

        builder.Property(x => x.AddressLine2)
            .HasConversion(
                addressLine2 => addressLine2 == null ? null : addressLine2.Value,
                value => AddressLine2.CreateOptional(value))
            .HasMaxLength(AddressLine2.MaxLength)
            .HasColumnName("AddressLine2");

        builder.Property(x => x.City)
            .HasConversion(
                city => city.Value,
                value => City.Create(value))
            .HasMaxLength(City.MaxLength)
            .HasColumnName("City")
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion(
                state => state == null ? null : state.Value,
                value => State.CreateOptional(value))
            .HasMaxLength(State.MaxLength)
            .HasColumnName("State");

        builder.Property(x => x.Country)
            .HasConversion(
                country => country.Value,
                value => Country.Create(value))
            .HasMaxLength(Country.MaxLength)
            .HasColumnName("Country")
            .IsRequired();

        builder.Property(x => x.PostalCode)
            .HasConversion(
                postalCode => postalCode.Value,
                value => PostalCode.Create(value))
            .HasMaxLength(PostalCode.MaxLength)
            .HasColumnName("PostalCode")
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(
                currencyCode => currencyCode.Value,
                value => CurrencyCode.Create(value))
            .HasMaxLength(CurrencyCode.MaxLength)
            .HasColumnName("CurrencyCode")
            .IsRequired();

        builder.Property(x => x.CreditLimit)
            .HasConversion(
                creditLimit => creditLimit.Value,
                value => CreditLimit.Create(value))
            .HasColumnType("decimal(18,2)")
            .HasColumnName("CreditLimit")
            .IsRequired();

        builder.Property(x => x.PaymentTermDays)
            .HasConversion(
                paymentTermDays => paymentTermDays.Value,
                value => PaymentTermDays.Create(value))
            .HasColumnName("PaymentTermDays")
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex("TenantId", "CustomerCode")
            .HasDatabaseName("UX_Customers_Tenant_CustomerCode")
            .IsUnique();

        builder.HasIndex("TenantId", "TaxNumber")
            .HasDatabaseName("UX_Customers_Tenant_TaxNumber")
            .IsUnique()
            .HasFilter("[TaxNumber] IS NOT NULL");

        builder.HasIndex("TenantId", "RegistrationNumber")
            .HasDatabaseName("UX_Customers_Tenant_RegistrationNumber")
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("IX_Customers_IsActive");

    }
}
