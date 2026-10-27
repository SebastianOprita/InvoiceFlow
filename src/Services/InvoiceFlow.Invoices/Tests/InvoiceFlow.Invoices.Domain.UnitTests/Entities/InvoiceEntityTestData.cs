namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

internal static class InvoiceEntityTestData
{
    internal static readonly Guid TenantId = Guid.CreateVersion7();
    internal static readonly DateTimeOffset CreatedAt = new(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
    internal static CurrencyCode RON => CurrencyCode.Create("RON");

    internal static InvoiceCustomerSnapshot Customer(Guid? tenantId = null, bool active = true, int paymentTerms = 30) =>
        InvoiceCustomerSnapshot.Create(
            tenantId ?? TenantId, Guid.CreateVersion7(), active,
            CustomerCode.Create("CUST-001"), CustomerName.Create("Acme Ltd"),
            CustomerEmail.Create("contact@example.test"), PhoneNumber.Create("+40700111222"),
            TaxNumber.Create("RO123456"), RegistrationNumber.Create("J40/123/2024"),
            AddressLine1.Create("Main Street 1"), AddressLine2.Create("Building A"),
            City.Create("Bucharest"), State.Create("Bucharest"), Country.Create("Romania"),
            PostalCode.Create("010101"), RON, CreditLimit.Create(1000m), PaymentTermDays.Create(paymentTerms));

    internal static Invoice Draft(InvoiceCustomerSnapshot? customer = null) =>
        Invoice.Create(TenantId, Guid.CreateVersion7(), customer ?? Customer(), InvoiceNotes.Create("Notes"), CreatedAt);

    internal static InvoiceLine AddLine(Invoice invoice, decimal unitPrice = 100m, decimal quantity = 1m,
        decimal discount = 0m, decimal tax = 19m, Guid? id = null) => invoice.AddManualLine(
        id ?? Guid.CreateVersion7(), InvoiceLineDescription.Create("Service"), Quantity.Create(quantity),
        UnitOfMeasure.Service, Money.Create(unitPrice, invoice.CurrencyCode), Percentage.Create(discount),
        Percentage.Create(tax), CreatedAt.AddHours(1));

    internal static InvoiceProductSnapshot Product(Guid? tenantId = null, bool active = true,
        CurrencyCode? currency = null, ProductDescription? description = null, decimal price = 100m) =>
        InvoiceProductSnapshot.Create(tenantId ?? TenantId, Guid.CreateVersion7(), active,
            ProductCode.Create("PROD-001"), ProductName.Create("Service"), description,
            UnitOfMeasure.Service, Money.Create(price, currency ?? RON), Percentage.Create(19m));

    internal static Invoice Issued()
    {
        var invoice = Draft();
        AddLine(invoice, id: Guid.CreateVersion7());
        invoice.Issue(InvoiceNumber.Create("INV-2025-000001"), new DateOnly(2025, 1, 2), CreatedAt.AddDays(1));
        return invoice;
    }
}
