using InvoiceFlow.BuildingBlocks.Domain;
using FluentAssertions;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.Entities;

public sealed class CustomerTests
{
    private readonly DateTime _now = DateTime.UtcNow;
    private static Customer CreateValidCustomer(
        DateTime dateTime,
        Guid? id = null,
        Guid? tenantId = null)
    {
        return Customer.Create(
            id ?? Guid.CreateVersion7(),
            tenantId ?? Guid.CreateVersion7(),
            CustomerCode.Create("CUST-001"),
            CustomerName.Create("ACME Corp"),
            CustomerEmail.Create("info@acme.com"),
            PhoneNumber.Create("1234567890"),
            null,
            RegistrationNumber.Create("REG-001"),
            AddressLine1.Create("123 Main St"),
            null,
            City.Create("Springfield"),
            null,
            Country.Create("US"),
            PostalCode.Create("12345"),
            CurrencyCode.Create("USD"),
            CreditLimit.Create(10000),
            PaymentTermDays.Create(30),
            dateTime);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var customer = CreateValidCustomer(_now);

        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.TenantId.Should().NotBeEmpty();
        customer.IsActive.Should().BeTrue();
        customer.CreatedAtUtc.Should().Be(_now);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Action act = () =>
            Customer.Create(
                Guid.CreateVersion7(),
                Guid.Empty,
                CustomerCode.Create("CUST-001"),
                CustomerName.Create("ACME Corp"),
                CustomerEmail.Create("info@acme.com"),
                PhoneNumber.Create("1234567890"),
                null,
                RegistrationNumber.Create("REG-001"),
                AddressLine1.Create("123 Main St"),
                null,
                City.Create("Springfield"),
                null,
                Country.Create("US"),
                PostalCode.Create("12345"),
                CurrencyCode.Create("USD"),
                CreditLimit.Create(10000),
                PaymentTermDays.Create(30),
                _now);

        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(DomainErrors.TenantIdRequired.ErrorCode);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Action act = () =>
            Customer.Create(
                Guid.Empty,
                Guid.CreateVersion7(),
                CustomerCode.Create("CUST-001"),
                CustomerName.Create("ACME Corp"),
                CustomerEmail.Create("info@acme.com"),
                PhoneNumber.Create("1234567890"),
                null,
                RegistrationNumber.Create("REG-001"),
                AddressLine1.Create("123 Main St"),
                null,
                City.Create("Springfield"),
                null,
                Country.Create("US"),
                PostalCode.Create("12345"),
                CurrencyCode.Create("USD"),
                CreditLimit.Create(10000),
                PaymentTermDays.Create(30),
                _now);

        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(DomainErrors.IdRequired.ErrorCode);
    }

    [Fact]
    public void Constructor_WithDefaultCreatedAtUtc_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Action act = () =>
            Customer.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CustomerCode.Create("CUST-001"),
                CustomerName.Create("ACME Corp"),
                CustomerEmail.Create("info@acme.com"),
                PhoneNumber.Create("1234567890"),
                null,
                RegistrationNumber.Create("REG-001"),
                AddressLine1.Create("123 Main St"),
                null,
                City.Create("Springfield"),
                null,
                Country.Create("US"),
                PostalCode.Create("12345"),
                CurrencyCode.Create("USD"),
                CreditLimit.Create(10000),
                PaymentTermDays.Create(30),
                default);

        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(DomainErrors.CreatedAtUtcRequired.ErrorCode);
    }

    #endregion
}
