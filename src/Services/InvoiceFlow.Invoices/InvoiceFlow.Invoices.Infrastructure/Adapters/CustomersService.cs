using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class CustomersService : ICustomersService
{
    private static readonly Dictionary<string, CustomerData> Customers =
        new Dictionary<string, CustomerData>(StringComparer.Ordinal)
        {
            ["CUST001"] = new(
                Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
                true,
                "Customer One",
                "one@email.com",
                "RON"),
            ["CUST002"] = new(
                Guid.Parse("a7f8c463-d740-4e12-98ae-c43b33745e62"),
                false,
                "Customer Two",
                "two@email.com",
                "RON"),
            ["CUST003"] = new(
                Guid.Parse("2d1867b8-b07e-4d10-8331-41c70edc0259"),
                true,
                "Customer three",
                "three@email.com",
                "USD")
        };

    public Task<Result<InvoiceCustomerSnapshot>> GetCustomerAsync(
        Guid tenantId,
        string customerCode,
        CancellationToken cancellationToken)
    {
        if (!Customers.TryGetValue(customerCode, out var data))
        {
            return Task.FromResult(
                Result<InvoiceCustomerSnapshot>.Failure(ApplicationErrors.CustomerNotFound));
        }

        var customer = InvoiceCustomerSnapshot.Create(
            customerTenantId: tenantId,
            customerId: data.Id,
            isActive: data.IsActive,
            customerCode: CustomerCode.Create(customerCode),
            customerName: CustomerName.Create(data.Name),
            customerEmail: CustomerEmail.Create(data.Email),
            customerPhone: PhoneNumber.CreateOptional("+1234567890"),
            customerTaxNumber: TaxNumber.CreateOptional("123456789"),
            customerRegistrationNumber: RegistrationNumber.Create("REG123456"),
            customerAddressLine1: AddressLine1.Create("123 Main St"),
            customerAddressLine2: AddressLine2.CreateOptional("Suite 100"),
            customerCity: City.Create("Cityville"),
            customerState: State.CreateOptional("Stateville"),
            customerCountry: Country.Create("Countryland"),
            customerPostalCode: PostalCode.Create("12345"),
            currencyCode: CurrencyCode.Create(data.CurrencyCode),
            creditLimit: CreditLimit.Create(10000m),
            paymentTermDays: PaymentTermDays.Create(30));

        return Task.FromResult(Result<InvoiceCustomerSnapshot>.Success(customer));
    }

    private sealed record CustomerData(
        Guid Id,
        bool IsActive,
        string Name,
        string Email,
        string CurrencyCode);
}
