using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Infrastructure;

public static class PaymentsDbSeed
{
    public static async Task SeedAsync(
        PaymentsDbContext db,
        BuildingBlocks.Application.ISystemDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a");

        var payment1 = Payment.Create(
            tenantId: tenantId,
            id: Guid.Parse("d1f3e3f0-e634-470b-b9c1-cc0c7559dc0a"),
            customerId: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            currencyCode: CurrencyCode.Create("RON"),
            amount: Money.Create(100m, CurrencyCode.Create("RON")),
            method: PaymentMethod.Card,
            reference: PaymentReference.CreateOptional("Payment reference."),
            notes: PaymentNotes.CreateOptional("Payment notes."),
            createdAtUtc: dateTimeProvider.GetUtcNow());

        payment1.AddAllocation(
            allocationId: Guid.Parse("e1f3e3f0-e634-470b-b9c1-cc0c7559dc0a"),
            invoiceId: Guid.Parse("a1f3e3f0-e634-470b-b9c1-cc0c7559dc0a"),
            invoiceNumber: InvoiceNumber.Create("INV001"),
            amount: Money.Create(100m, CurrencyCode.Create("RON")),
            updatedAtUtc: dateTimeProvider.GetUtcNow());

        var payment2 = Payment.Create(
            tenantId: tenantId,
            id: Guid.Parse("d1f3e3f0-e634-470b-b9c1-cc0c7559dc0b"),
            customerId: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            currencyCode: CurrencyCode.Create("RON"),
            amount: Money.Create(100m, CurrencyCode.Create("RON")),
            method: PaymentMethod.Card,
            reference: PaymentReference.CreateOptional("Payment reference."),
            notes: PaymentNotes.CreateOptional("Payment notes."),
            createdAtUtc: dateTimeProvider.GetUtcNow());

        db.Payments.Add(payment1);
        db.Payments.Add(payment2);
        await db.SaveChangesAsync(cancellationToken);
    }
}
