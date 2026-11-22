using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T10:00:00Z");
    private static readonly CurrencyCode Ron = CurrencyCode.Create("RON");
    private static readonly Guid Actor = Guid.CreateVersion7();

    private static Payment NewPayment(decimal amount = 100m) => Payment.Create(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Ron,
        Money.Create(amount, Ron), PaymentMethod.Card,
        PaymentReference.CreateOptional(" ref "), PaymentNotes.CreateOptional(" notes "), Actor, Created);

    private static InvoicePaymentBalance NewBalance(Payment payment, decimal amount = 100m) => InvoicePaymentBalance.Create(
        payment.TenantId, Guid.CreateVersion7(), payment.CustomerId,
        InvoiceNumber.Create("INV-1"), payment.CurrencyCode, Money.Create(amount, payment.CurrencyCode), Created);

    private static void Post(Payment payment, DateTimeOffset? at = null) =>
        payment.Post(PaymentNumber.Create("PAY-2025-1"), new DateOnly(2025, 1, 2), Actor, at ?? Created.AddHours(1));

    [Fact]
    public void Create_sets_draft_state_and_audit_actor()
    {
        var payment = NewPayment();
        payment.Status.Should().Be(PaymentLifecycleStatus.Draft);
        payment.CreatedBy.Should().Be(Actor);
        payment.AllocatedAmount.Amount.Should().Be(0m);
        payment.UnallocatedAmount.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_rejects_missing_creator()
    {
        Action act = () => Payment.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            Ron, Money.Create(10m, Ron), PaymentMethod.Card, null, null, Guid.Empty, Created);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CreatedByRequired);
    }

    [Fact]
    public void AddAllocation_updates_payment_and_invoice_balance()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        Post(payment);

        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(60m, Ron), Actor, Created.AddHours(2));

        payment.AllocatedAmount.Amount.Should().Be(60m);
        payment.UnallocatedAmount.Amount.Should().Be(40m);
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(40m);
        payment.Allocations.Single().CreatedBy.Should().Be(Actor);
    }

    [Fact]
    public void ReverseAllocation_preserves_audit_history_and_restores_balances()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(60m, Ron), Actor, Created.AddHours(2));

        payment.ReverseAllocation(allocationId, balance, Actor, " Duplicate receipt ", Created.AddHours(3));

        payment.AllocatedAmount.Amount.Should().Be(0m);
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.Allocations.Should().ContainSingle();
        payment.GetAllocation(allocationId).IsReversed.Should().BeTrue();
        payment.GetAllocation(allocationId).ReversalReason!.Value.Should().Be("Duplicate receipt");
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        balance.OutstandingAmount.Amount.Should().Be(100m);
    }

    [Fact]
    public void ReverseAllocation_rejects_reversing_the_same_allocation_twice()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(25m, Ron), Actor, Created.AddHours(2));
        payment.ReverseAllocation(allocationId, balance, Actor, "Correction", Created.AddHours(3));

        Action act = () => payment.ReverseAllocation(allocationId, balance, Actor, "Again", Created.AddHours(4));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationAlreadyReversed);
    }

    [Fact]
    public void ReverseAllocation_rejects_a_balance_for_another_invoice_without_mutating_state()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var differentBalance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(25m, Ron), Actor, Created.AddHours(2));

        Action act = () => payment.ReverseAllocation(allocationId, differentBalance, Actor, "Correction", Created.AddHours(3));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceIdentityMismatch);
        payment.GetAllocation(allocationId).IsReversed.Should().BeFalse();
        balance.AllocatedAmount.Amount.Should().Be(25m);
        differentBalance.AllocatedAmount.Amount.Should().Be(0m);
    }

    [Fact]
    public void ReverseAllocation_rejects_a_timestamp_before_last_payment_update()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        Post(payment);
        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(10m, Ron), Actor, Created.AddHours(2));

        Action act = () => payment.ReverseAllocation(payment.Allocations.Single().Id, balance, Actor, "Correction", Created.AddHours(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcInvalid);
    }
}
