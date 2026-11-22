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

        var createdAtUtc = dateTimeProvider.GetUtcNow();
        var payment1 = Payment.Create(
            tenantId: tenantId,
            id: Guid.Parse("d1f3e3f0-e634-470b-b9c1-cc0c7559dc0a"),
            customerId: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            currencyCode: CurrencyCode.Create("RON"),
            amount: Money.Create(100m, CurrencyCode.Create("RON")),
            method: PaymentMethod.Card,
            reference: PaymentReference.CreateOptional("Payment reference."),
            notes: PaymentNotes.CreateOptional("Payment notes."),
            createdBy: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            createdAtUtc: createdAtUtc);
        payment1.Post(
            PaymentNumber.Create("PAY-2025-000001"),
            DateOnly.FromDateTime(createdAtUtc.UtcDateTime),
            Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            createdAtUtc);

        var payment2 = Payment.Create(
            tenantId: tenantId,
            id: Guid.Parse("d1f3e3f0-e634-470b-b9c1-cc0c7559dc0b"),
            customerId: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            currencyCode: CurrencyCode.Create("RON"),
            amount: Money.Create(100m, CurrencyCode.Create("RON")),
            method: PaymentMethod.Card,
            reference: PaymentReference.CreateOptional("Payment reference."),
            notes: PaymentNotes.CreateOptional("Payment notes."),
            createdBy: Guid.Parse("722fa11b-19d1-4184-898d-9fe814b32f28"),
            createdAtUtc: createdAtUtc);

        db.Payments.Add(payment1);
        db.Payments.Add(payment2);
        await db.SaveChangesAsync(cancellationToken);
    }
}
