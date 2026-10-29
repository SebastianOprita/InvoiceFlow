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

        var product = InvoiceProductSnapshot.Create(
            productTenantId: tenantId,
            productId: Guid.Parse("cda14eb7-598c-4c66-ad4d-d0db7f11bad7"),
            isActive: true,
            productCode: ProductCode.Create("product001"),
            productName: ProductName.Create("product name"),
            productDescription: ProductDescription.Create("product description"),
            unitOfMeasure: UnitOfMeasure.Piece,
            unitPrice: Money.Create(12m, CurrencyCode.Create("RON")),
            taxRate: Percentage.Create(9m));

        var invoice = Invoice.Create(
            tenantId: tenantId,
            Guid.CreateVersion7(),
            customer,
            InvoiceNotes.CreateOptional("Invoice notes."),
            dateTimeProvider.GetUtcNow());

        invoice.AddProductLine(
            Guid.CreateVersion7(),
            product,
            Quantity.Create(100),
            Percentage.Create(10),
            dateTimeProvider.GetUtcNow());

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }
}
