using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Application.UnitTests;

internal static class TestConstants
{
    internal static readonly Guid TenantId = Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a");
    internal static Customer Customer(Guid tenantId, Guid customerId, string name, DateTime dateTime) => Domain.Customer.Create
    (
        customerId,
        tenantId,
        CustomerCode.Create("CustomerCode"),
        CustomerName.Create(name),
        CustomerEmail.Create("CustomerEmail@email.com"),
        PhoneNumber.Create("1234567890"),
        TaxNumber.Create("TAX-123"),
        RegistrationNumber.Create("REG-123"),
        AddressLine1.Create("123 Main St"),
        AddressLine2.Create("Suite 100"),
        City.Create("City"),
        State.Create("State"),
        Country.Create("Country"),
        PostalCode.Create("12345"),
        CurrencyCode.Create("USD"),
        CreditLimit.Create(10000m),
        PaymentTermDays.Create(30),
        dateTime
    );

    internal static CreateCustomerCommand CreateCustomerCommand(Guid tenantId, string name = "CustomerName")
        => new CreateCustomerCommand
        (
            tenantId,
            "CustomerCode",
            name,
            new CustomerContact("CustomerEmail@email.com", "1234567890"),
            new CustomerTaxDetails("1234567890", "TAX-123"),
            new CustomerAddress
            (
                "123 Main St",
                "Suite 100",
                "City",
                "State",
                "Country",
                "12345"
            ),
            "USD",
            new CustomerCreditPolicy(10000m, 30)
        );

    internal static UpdateCustomerCommand UpdateCustomerCommand(Guid tenantId, Guid customerId, string name = "CustomerName")
        => new UpdateCustomerCommand
        (
            tenantId,
            customerId,
            name,
            new CustomerContact("CustomerEmail@email.com", "1234567890"),
            new CustomerAddress
            (
                "123 Main St",
                "Suite 100",
                "City",
                "State",
                "Country",
                "12345"
            ),
            new CustomerCreditPolicy(10000m, 30)
        );
}
