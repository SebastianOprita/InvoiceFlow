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
        var tenant = Guid.CreateVersion7(); var id = Guid.CreateVersion7(); var payment = Guid.CreateVersion7(); var invoice = Guid.CreateVersion7();
        var localTimestamp = new DateTimeOffset(2025, 1, 1, 3, 0, 0, TimeSpan.FromHours(3));
        var allocation = PaymentAllocation.Create(tenant, id, payment, invoice, InvoiceNumber.Create(" inv-1 "), Money.Create(12.5m, CurrencyCode.Create("RON")), localTimestamp);
        allocation.TenantId.Should().Be(tenant);
        allocation.Id.Should().Be(id);
        allocation.PaymentId.Should().Be(payment);
        allocation.InvoiceId.Should().Be(invoice);
        allocation.InvoiceNumber.Value.Should().Be("INV-1");
        allocation.Amount.Amount.Should().Be(12.5m);
        allocation.CreatedAtUtc.Should().Be(localTimestamp.ToUniversalTime());
    }

    [Fact]
    public void Create_rejects_empty_identifiers_invalid_amount_and_missing_timestamp()
    {
        var tenant = Guid.CreateVersion7(); var id = Guid.CreateVersion7(); var payment = Guid.CreateVersion7(); var invoice = Guid.CreateVersion7();
        Action emptyTenant = () => PaymentAllocation.Create(Guid.Empty, id, payment, invoice, InvoiceNumber.Create("INV-1"), Money.Create(1m, CurrencyCode.Create("RON")), Created);
        emptyTenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
        Action emptyPayment = () => PaymentAllocation.Create(tenant, id, Guid.Empty, invoice, InvoiceNumber.Create("INV-1"), Money.Create(1m, CurrencyCode.Create("RON")), Created);
        emptyPayment.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentIdRequired);
        Action emptyInvoice = () => PaymentAllocation.Create(tenant, id, payment, Guid.Empty, InvoiceNumber.Create("INV-1"), Money.Create(1m, CurrencyCode.Create("RON")), Created);
        emptyInvoice.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceIdRequired);
        Action zeroAmount = () => PaymentAllocation.Create(tenant, id, payment, invoice, InvoiceNumber.Create("INV-1"), Money.Zero(CurrencyCode.Create("RON")), Created);
        zeroAmount.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationAmountInvalid);
        Action missingDate = () => PaymentAllocation.Create(tenant, id, payment, invoice, InvoiceNumber.Create("INV-1"), Money.Create(1m, CurrencyCode.Create("RON")), default);
        missingDate.Should().Throw<DomainException>().WithDomainError(DomainErrors.CreatedAtUtcInvalid);
    }
}
