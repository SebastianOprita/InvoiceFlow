using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application.UnitTests;

internal static class InvoiceTestData
{
    internal static readonly Guid TenantId = Guid.CreateVersion7();
    internal static InvoiceCustomerSnapshot Customer(Guid? tenantId = null) => InvoiceCustomerSnapshot.Create(
        tenantId ?? TenantId, Guid.CreateVersion7(), true,
        CustomerCode.Create("CUST-001"), CustomerName.Create("Acme Ltd"),
        CustomerEmail.Create("contact@acme.test"), PhoneNumber.Create("+40700111222"),
        TaxNumber.Create("RO123456"), RegistrationNumber.Create("J40/123/2024"),
        AddressLine1.Create("Main Street 1"), AddressLine2.Create("Building A"),
        City.Create("Bucharest"), State.Create("Bucharest"), Country.Create("Romania"),
        PostalCode.Create("010101"), CurrencyCode.Create("RON"), CreditLimit.Create(1000m), PaymentTermDays.Create(30));

    internal static Invoice DraftInvoice()
    {
        var now = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var invoice = Invoice.Create(TenantId, Guid.CreateVersion7(), Customer(), InvoiceNotes.Create("Invoice notes"), now);
        invoice.AddProductLine(Guid.CreateVersion7(), InvoiceProductSnapshot.Create(
            TenantId, Guid.CreateVersion7(), true, ProductCode.Create("PROD-001"), ProductName.Create("Service"),
            ProductDescription.Create("Professional service"), UnitOfMeasure.Service,
            Money.Create(100, CurrencyCode.Create("RON")), Percentage.Create(19)), Quantity.Create(1), Percentage.Create(0), now);
        return invoice;
    }
}
