using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class CustomersService : ICustomersService
{
    public Task<Result<InvoiceCustomerSnapshot>> GetCustomerAsync(Guid tenantId, string customerCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return Task.FromResult(
                Result<InvoiceCustomerSnapshot>.Failure(
                    ApplicationErrors.CustomerNotFound));

        var customer = InvoiceCustomerSnapshot.Create(
            customerTenantId: tenantId,
            customerId: Guid.Parse("01a0cae9-2e61-7b82-924b-5beaf5b08d4a"),
            isActive: true,
            customerCode: CustomerCode.Create("CUST001"),
            customerName: CustomerName.Create("Customer One"),
            customerEmail: CustomerEmail.Create("one@email.com"),
            customerPhone: PhoneNumber.CreateOptional("+1234567890"),
            customerTaxNumber: TaxNumber.CreateOptional("123456789"),
            customerRegistrationNumber: RegistrationNumber.Create("REG123456"),
            customerAddressLine1: AddressLine1.Create("123 Main St"),
            customerAddressLine2: AddressLine2.CreateOptional("Suite 100"),
            customerCity: City.Create("Cityville"),
            customerState: State.CreateOptional("Stateville"),
            customerCountry: Country.Create("Countryland"),
            customerPostalCode: PostalCode.Create("12345"),
            currencyCode: CurrencyCode.Create("USD"),
            creditLimit: CreditLimit.Create(10000m),
            paymentTermDays: PaymentTermDays.Create(30));

        return Task.FromResult(
            Result<InvoiceCustomerSnapshot>.Success(customer));

    }
}
