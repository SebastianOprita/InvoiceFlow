using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class CustomersService : ICustomersService
{
    public Task<Result<InvoiceCustomerSnapshot>> GetCustomerAsync(Guid tenantId, string customerCode, CancellationToken cancellationToken)
    {
        if (customerCode == "CUST001")
        {
            var customer = InvoiceCustomerSnapshot.Create(
                customerTenantId: tenantId,
                customerId: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
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
                currencyCode: CurrencyCode.Create("RON"),
                creditLimit: CreditLimit.Create(10000m),
                paymentTermDays: PaymentTermDays.Create(30));

            return Task.FromResult(
                Result<InvoiceCustomerSnapshot>.Success(customer));
        }

        if (customerCode == "CUST002")
        {
            var customer = InvoiceCustomerSnapshot.Create(
                customerTenantId: tenantId,
                customerId: Guid.Parse("a7f8c463-d740-4e12-98ae-c43b33745e62"),
                isActive: false,
                customerCode: CustomerCode.Create("CUST002"),
                customerName: CustomerName.Create("Customer Two"),
                customerEmail: CustomerEmail.Create("two@email.com"),
                customerPhone: PhoneNumber.CreateOptional("+1234567890"),
                customerTaxNumber: TaxNumber.CreateOptional("123456789"),
                customerRegistrationNumber: RegistrationNumber.Create("REG123456"),
                customerAddressLine1: AddressLine1.Create("123 Main St"),
                customerAddressLine2: AddressLine2.CreateOptional("Suite 100"),
                customerCity: City.Create("Cityville"),
                customerState: State.CreateOptional("Stateville"),
                customerCountry: Country.Create("Countryland"),
                customerPostalCode: PostalCode.Create("12345"),
                currencyCode: CurrencyCode.Create("RON"),
                creditLimit: CreditLimit.Create(10000m),
                paymentTermDays: PaymentTermDays.Create(30));

            return Task.FromResult(
                Result<InvoiceCustomerSnapshot>.Success(customer));
        }

        if (customerCode == "CUST003")
        {
            var customer = InvoiceCustomerSnapshot.Create(
                customerTenantId: tenantId,
                customerId: Guid.Parse("2d1867b8-b07e-4d10-8331-41c70edc0259"),
                isActive: true,
                customerCode: CustomerCode.Create("CUST003"),
                customerName: CustomerName.Create("Customer three"),
                customerEmail: CustomerEmail.Create("three@email.com"),
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

        return Task.FromResult(
            Result<InvoiceCustomerSnapshot>.Failure(
                ApplicationErrors.CustomerNotFound));
    }
}
