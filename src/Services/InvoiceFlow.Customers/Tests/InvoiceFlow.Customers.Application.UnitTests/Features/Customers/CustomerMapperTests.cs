using FluentAssertions;
using InvoiceFlow.Customers.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers;

public sealed class CustomerMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCustomerToCustomerDto()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var customer = CreateCustomer(createdAtUtc: createdAtUtc);

        // Act
        var dto = customer.ToDto();

        // Assert
        dto.TenantId.Should().Be(customer.TenantId);
        dto.Id.Should().Be(customer.Id);
        dto.Name.Should().Be(customer.Name.Value);
        dto.CustomerCode.Should().Be(customer.CustomerCode.Value);
        dto.CurrencyCode.Should().Be(customer.CurrencyCode.Value);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(createdAtUtc);
        dto.UpdatedAtUtc.Should().BeNull();

        dto.CustomerContact.Should().BeEquivalentTo(new CustomerContactDto
        {
            Email = customer.Email!.Value,
            Phone = customer.Phone!.Value
        });

        dto.CustomerTaxDetails.Should().BeEquivalentTo(new CustomerTaxDetailsDto
        {
            TaxNumber = customer.TaxNumber!.Value,
            RegistrationNumber = customer.RegistrationNumber.Value
        });

        dto.CustomerAddress.Should().BeEquivalentTo(new CustomerAddressDto
        {
            AddressLine1 = customer.AddressLine1.Value,
            AddressLine2 = customer.AddressLine2!.Value,
            City = customer.City.Value,
            State = customer.State!.Value,
            Country = customer.Country.Value,
            PostalCode = customer.PostalCode.Value
        });

        dto.CustomerCreditPolicy.Should().BeEquivalentTo(new CustomerCreditPolicyDto
        {
            CreditLimit = customer.CreditLimit.Value,
            PaymentTermDays = customer.PaymentTermDays.Value
        });
    }

    [Fact]
    public void ToDto_ShouldMapNullableFieldsAsNull()
    {
        // Arrange
        var customer = CreateCustomer(
            withEmail: false,
            withPhone: false,
            withTaxNumber: false,
            withAddressLine2: false,
            withState: false);

        // Act
        var dto = customer.ToDto();

        // Assert
        dto.CustomerContact.Email.Should().BeNull();
        dto.CustomerContact.Phone.Should().BeNull();
        dto.CustomerTaxDetails.TaxNumber.Should().BeNull();
        dto.CustomerAddress.AddressLine2.Should().BeNull();
        dto.CustomerAddress.State.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapUpdatedAtUtc_WhenCustomerWasUpdated()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAtUtc = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        var customer = CreateCustomer(createdAtUtc: createdAtUtc);

        customer.UpdateName(CustomerName.Create("Updated Customer"), updatedAtUtc);

        // Act
        var dto = customer.ToDto();

        // Assert
        dto.Name.Should().Be("Updated Customer");
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapInactiveCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        var updatedAtUtc = customer.CreatedAtUtc.AddDays(1);

        customer.Deactivate(updatedAtUtc);

        // Act
        var dto = customer.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    private static Customer CreateCustomer(
        DateTime? createdAtUtc = null,
        bool withEmail = true,
        bool withPhone = true,
        bool withTaxNumber = true,
        bool withAddressLine2 = true,
        bool withState = true)
    {
        return Customer.Create(
            id: Guid.CreateVersion7(),
            tenantId: Guid.CreateVersion7(),
            customerCode: CustomerCode.Create("CUST-001"),
            name: CustomerName.Create("Acme Ltd"),
            email: withEmail ? CustomerEmail.Create("contact@acme.com") : null,
            phone: withPhone ? PhoneNumber.Create("+40700111222") : null,
            taxNumber: withTaxNumber ? TaxNumber.Create("RO123456") : null,
            registrationNumber: RegistrationNumber.Create("J40/123/2024"),
            addressLine1: AddressLine1.Create("Main Street 1"),
            addressLine2: withAddressLine2 ? AddressLine2.Create("Building A") : null,
            city: City.Create("Bucharest"),
            state: withState ? State.Create("Bucharest") : null,
            country: Country.Create("Romania"),
            postalCode: PostalCode.Create("010101"),
            currencyCode: CurrencyCode.Create("RON"),
            creditLimit: CreditLimit.Create(10000),
            paymentTermDays: PaymentTermDays.Create(30),
            createdAtUtc: createdAtUtc ?? new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
