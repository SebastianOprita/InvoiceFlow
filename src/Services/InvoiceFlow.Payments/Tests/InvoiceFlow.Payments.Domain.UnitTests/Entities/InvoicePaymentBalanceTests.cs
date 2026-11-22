using FluentAssertions;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class InvoicePaymentBalanceTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T00:00:00Z");

    [Fact]
    public void Create_starts_open_and_unpaid()
    {
        var balance = InvoicePaymentBalance.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            InvoiceNumber.Create("INV-1"), CurrencyCode.Create("RON"),
            Money.Create(100m, CurrencyCode.Create("RON")), Created);

        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        balance.OutstandingAmount.Amount.Should().Be(100m);
        balance.Version.Should().Be(0);
    }

    [Fact]
    public void Payment_allocation_and_reversal_update_balance_status_and_version()
    {
        var tenantId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var currency = CurrencyCode.Create("RON");
        var payment = Payment.Create(tenantId, Guid.CreateVersion7(), customerId, currency,
            Money.Create(100m, currency), PaymentMethod.Card, null, null, Guid.CreateVersion7(), Created);
        var balance = InvoicePaymentBalance.Create(tenantId, Guid.CreateVersion7(), customerId,
            InvoiceNumber.Create("INV-1"), currency, Money.Create(100m, currency), Created);
        payment.Post(PaymentNumber.Create("PAY-2025-1"), new DateOnly(2025, 1, 2), Guid.CreateVersion7(), Created.AddHours(1));

        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(25m, currency), Guid.CreateVersion7(), Created.AddHours(2));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(75m);
        balance.Version.Should().Be(1);

        var secondPayment = Payment.Create(tenantId, Guid.CreateVersion7(), customerId, currency,
            Money.Create(100m, currency), PaymentMethod.Card, null, null, Guid.CreateVersion7(), Created);
        secondPayment.Post(PaymentNumber.Create("PAY-2025-2"), new DateOnly(2025, 1, 2), Guid.CreateVersion7(), Created.AddHours(1));
        secondPayment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(75m, currency), Guid.CreateVersion7(), Created.AddHours(3));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.Paid);

        var allocation = secondPayment.Allocations.Single();
        secondPayment.ReverseAllocation(allocation.Id, balance, Guid.CreateVersion7(), "Correction", Created.AddHours(4));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(75m);
        balance.Version.Should().Be(3);
    }
}
