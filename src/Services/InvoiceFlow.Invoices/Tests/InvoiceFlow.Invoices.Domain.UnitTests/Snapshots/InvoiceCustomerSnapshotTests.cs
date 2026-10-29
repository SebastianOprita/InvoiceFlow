using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Snapshots;

public sealed class InvoiceCustomerSnapshotTests
{
    [Fact]
    public void Create_CopiesAllCustomerFields()
    {
        var tenantId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var customer = CreateCustomer(tenantId, customerId);

        customer.CustomerTenantId.Should().Be(tenantId);
        customer.CustomerId.Should().Be(customerId);
        customer.IsActive.Should().BeTrue();
        customer.CustomerCode.Value.Should().Be("CUST-001");
        customer.CustomerName.Value.Should().Be("Acme Ltd");
        customer.CustomerEmail!.Value.Should().Be("contact@example.test");
        customer.CustomerPhone!.Value.Should().Be("+40700111222");
        customer.CustomerTaxNumber!.Value.Should().Be("RO123456");
        customer.CustomerRegistrationNumber.Value.Should().Be("J40/123/2024");
        customer.CustomerAddressLine1.Value.Should().Be("Main Street 1");
        customer.CustomerAddressLine2!.Value.Should().Be("Building A");
        customer.CustomerCity.Value.Should().Be("Bucharest");
        customer.CustomerState!.Value.Should().Be("Bucharest");
        customer.CustomerCountry.Value.Should().Be("Romania");
        customer.CustomerPostalCode.Value.Should().Be("010101");
        customer.CurrencyCode.Value.Should().Be("RON");
        customer.CreditLimit.Value.Should().Be(1000m);
        customer.PaymentTermDays.Value.Should().Be(30);
    }

    [Fact]
    public void Create_PreservesNullOptionalCustomerFields()
    {
        var customer = InvoiceCustomerSnapshot.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), false,
            CustomerCode.Create("CUST-002"), CustomerName.Create("Second Customer"),
            customerEmail: null,
            customerPhone: null,
            customerTaxNumber: null,
            customerRegistrationNumber: RegistrationNumber.Create("REG-2"),
            customerAddressLine1: AddressLine1.Create("Second Street"),
            customerAddressLine2: null,
            customerCity: City.Create("Cluj"),
            customerState: null,
            customerCountry: Country.Create("Romania"),
            customerPostalCode: PostalCode.Create("400000"),
            currencyCode: CurrencyCode.Create("EUR"),
            creditLimit: CreditLimit.Create(0),
            paymentTermDays: PaymentTermDays.Create(0));

        customer.IsActive.Should().BeFalse();
        customer.CustomerEmail.Should().BeNull();
        customer.CustomerPhone.Should().BeNull();
        customer.CustomerTaxNumber.Should().BeNull();
        customer.CustomerAddressLine2.Should().BeNull();
        customer.CustomerState.Should().BeNull();
        customer.CurrencyCode.Value.Should().Be("EUR");
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        var customer = CreateCustomer(Guid.CreateVersion7(), Guid.CreateVersion7());

        Action act = () => InvoiceCustomerSnapshot.Create(
            Guid.Empty, customer.CustomerId, customer.IsActive,
            customer.CustomerCode, customer.CustomerName, customer.CustomerEmail, customer.CustomerPhone,
            customer.CustomerTaxNumber, customer.CustomerRegistrationNumber, customer.CustomerAddressLine1,
            customer.CustomerAddressLine2, customer.CustomerCity, customer.CustomerState, customer.CustomerCountry,
            customer.CustomerPostalCode, customer.CurrencyCode, customer.CreditLimit, customer.PaymentTermDays);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyCustomerId()
    {
        var customer = CreateCustomer(Guid.CreateVersion7(), Guid.CreateVersion7());

        Action act = () => InvoiceCustomerSnapshot.Create(
            customer.CustomerTenantId, Guid.Empty, customer.IsActive,
            customer.CustomerCode, customer.CustomerName, customer.CustomerEmail, customer.CustomerPhone,
            customer.CustomerTaxNumber, customer.CustomerRegistrationNumber, customer.CustomerAddressLine1,
            customer.CustomerAddressLine2, customer.CustomerCity, customer.CustomerState, customer.CustomerCountry,
            customer.CustomerPostalCode, customer.CurrencyCode, customer.CreditLimit, customer.PaymentTermDays);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    private static InvoiceCustomerSnapshot CreateCustomer(Guid tenantId, Guid customerId) => InvoiceCustomerSnapshot.Create(
        tenantId, customerId, true,
        CustomerCode.Create("CUST-001"), CustomerName.Create("Acme Ltd"),
        CustomerEmail.Create("contact@example.test"), PhoneNumber.Create("+40700111222"),
        TaxNumber.Create("RO123456"), RegistrationNumber.Create("J40/123/2024"),
        AddressLine1.Create("Main Street 1"), AddressLine2.Create("Building A"),
        City.Create("Bucharest"), State.Create("Bucharest"), Country.Create("Romania"),
        PostalCode.Create("010101"), CurrencyCode.Create("RON"), CreditLimit.Create(1000m), PaymentTermDays.Create(30));
}
