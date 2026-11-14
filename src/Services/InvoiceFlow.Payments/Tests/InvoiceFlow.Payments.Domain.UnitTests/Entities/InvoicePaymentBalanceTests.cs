using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class InvoicePaymentBalanceTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T00:00:00Z");
    private static InvoicePaymentBalance NewBalance() => InvoicePaymentBalance.Create(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceNumber.Create("INV-1"),
        CurrencyCode.Create("RON"), Money.Create(100m, CurrencyCode.Create("RON")), Created);

    [Fact]
    public void Create_starts_open_and_unpaid()
    {
        var balance = NewBalance();
        balance.Status.Should().Be(InvoicePaymentBalanceStatus.Open);
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        balance.OutstandingAmount.Amount.Should().Be(100m);
        balance.Version.Should().Be(0);
    }

    [Fact]
    public void Allocate_and_reverse_update_balance_status_and_version()
    {
        var balance = NewBalance();
        balance.Allocate(Money.Create(25m, CurrencyCode.Create("RON")), Created.AddHours(1));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(75m);
        balance.Version.Should().Be(1);

        balance.Allocate(Money.Create(75m, CurrencyCode.Create("RON")), Created.AddHours(2));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.Paid);
        balance.CanAllocate(Money.Create(1m, CurrencyCode.Create("RON"))).Should().BeFalse();

        balance.ReverseAllocation(Money.Create(10m, CurrencyCode.Create("RON")), Created.AddHours(3));
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(10m);
        balance.Version.Should().Be(3);
    }

    [Fact]
    public void Allocate_rejects_overallocation_and_currency_mismatch()
    {
        var balance = NewBalance();
        Action over = () => balance.Allocate(Money.Create(101m, CurrencyCode.Create("RON")), Created.AddHours(1));
        over.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceOverAllocation);
        Action mismatch = () => balance.Allocate(Money.Create(1m, CurrencyCode.Create("EUR")), Created.AddHours(1));
        mismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
        balance.CanAllocate(Money.Create(101m, CurrencyCode.Create("RON"))).Should().BeFalse();
    }

    [Fact]
    public void Reverse_rejects_more_than_allocated()
    {
        var balance = NewBalance();
        Action act = () => balance.ReverseAllocation(Money.Create(1m, CurrencyCode.Create("RON")), Created.AddHours(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceOverReversal);
    }

    [Fact]
    public void Close_requires_zero_allocations_and_prevents_future_changes()
    {
        var balance = NewBalance();
        balance.Allocate(Money.Create(10m, CurrencyCode.Create("RON")), Created.AddHours(1));
        Action hasAllocations = () => balance.Close(Created.AddHours(2));
        hasAllocations.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceHasAllocations);
        balance.ReverseAllocation(Money.Create(10m, CurrencyCode.Create("RON")), Created.AddHours(2));
        balance.Close(Created.AddHours(3));
        balance.Status.Should().Be(InvoicePaymentBalanceStatus.Closed);
        balance.Version.Should().Be(3);
        balance.Close(Created.AddHours(4));
        Action allocate = () => balance.Allocate(Money.Create(1m, CurrencyCode.Create("RON")), Created.AddHours(4));
        allocate.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceClosed);
    }
}
