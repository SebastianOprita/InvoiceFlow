using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class PaymentAllocationTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T00:00:00Z");

    [Fact]
    public void Create_sets_fields_and_normalizes_created_timestamp()
    {
        var tenant = Guid.CreateVersion7();
        var id = Guid.CreateVersion7();
        var payment = Guid.CreateVersion7();
        var invoice = Guid.CreateVersion7();
        var creator = Guid.CreateVersion7();
        var localTimestamp = new DateTimeOffset(2025, 1, 1, 3, 0, 0, TimeSpan.FromHours(3));
        var allocation = PaymentAllocation.Create(tenant, id, payment, invoice,
            InvoiceNumber.Create(" inv-1 "), Money.Create(12.5m, CurrencyCode.Create("RON")), creator, localTimestamp);

        allocation.TenantId.Should().Be(tenant);
        allocation.Id.Should().Be(id);
        allocation.PaymentId.Should().Be(payment);
        allocation.InvoiceId.Should().Be(invoice);
        allocation.CreatedBy.Should().Be(creator);
        allocation.InvoiceNumber.Value.Should().Be("INV-1");
        allocation.Amount.Amount.Should().Be(12.5m);
        allocation.CreatedAtUtc.Should().Be(localTimestamp.ToUniversalTime());
    }

    [Fact]
    public void Create_rejects_missing_creator()
    {
        Action act = () => PaymentAllocation.Create(Guid.CreateVersion7(), Guid.CreateVersion7(),
            Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceNumber.Create("INV-1"),
            Money.Create(1m, CurrencyCode.Create("RON")), Guid.Empty, Created);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CreatedByRequired);
    }
}
