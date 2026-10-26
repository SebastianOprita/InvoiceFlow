using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

public static class InvoicesDbSeed
{
    public static async Task SeedAsync(
        InvoicesDbContext db,
        BuildingBlocks.Application.ISystemDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a");

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
            creditLimit: CreditLimit.Create(1000m),
            paymentTermDays: PaymentTermDays.Create(30));

        var invoice = Invoice.Create(
            tenantId: tenantId,
            Guid.CreateVersion7(),
            customer,
            InvoiceNotes.CreateOptional("Invoice notes."),
            dateTimeProvider.GetUtcNow());

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }
}
