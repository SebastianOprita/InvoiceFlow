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

    [Fact]
    public void ChangeAmount_updates_amount_and_audit_fields()
    {
        var payment = NewPayment();
        payment.ChangeAmount(Money.Create(125m, Ron), Actor, Created.AddHours(1));

        payment.Amount.Amount.Should().Be(125m);
        payment.UpdatedBy.Should().Be(Actor);
        payment.UpdatedAtUtc.Should().Be(Created.AddHours(1));
    }

    [Fact]
    public void ChangeAmount_rejects_modification_after_posting()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        Post(payment);
        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(40m, Ron), Actor, Created.AddHours(2));
        Action act = () => payment.ChangeAmount(Money.Create(30m, Ron), Actor, Created.AddHours(3));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CanNotBeModified);
    }

    [Fact]
    public void ChangeAmount_rejects_invalid_amount_and_currency()
    {
        var payment = NewPayment();

        Action invalidAmount = () => payment.ChangeAmount(Money.Zero(Ron), Actor, Created.AddHours(1));
        invalidAmount.Should().Throw<DomainException>().WithDomainError(DomainErrors.AmountInvalid);

        Action currencyMismatch = () => payment.ChangeAmount(Money.Create(10m, CurrencyCode.Create("EUR")), Actor, Created.AddHours(1));
        currencyMismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Fact]
    public void ChangePaymentDetails_updates_all_fields_and_audit_values()
    {
        var payment = NewPayment();
        var reference = PaymentReference.Create("new reference");
        var notes = PaymentNotes.Create("new notes");
        payment.ChangePaymentDetails(PaymentMethod.BankTransfer, reference, notes, Actor, Created.AddHours(1));

        payment.Method.Should().Be(PaymentMethod.BankTransfer);
        payment.Reference.Should().Be(reference);
        payment.Notes.Should().Be(notes);
        payment.UpdatedBy.Should().Be(Actor);
        payment.UpdatedAtUtc.Should().Be(Created.AddHours(1));
    }

    [Fact]
    public void ChangeMethod_updates_value_and_audit_but_same_value_is_a_noop()
    {
        var payment = NewPayment();
        payment.ChangeMethod(PaymentMethod.BankTransfer, Actor, Created.AddHours(1));
        payment.Method.Should().Be(PaymentMethod.BankTransfer);
        payment.UpdatedBy.Should().Be(Actor);

        var unchanged = NewPayment();
        unchanged.ChangeMethod(PaymentMethod.Card, Actor, Created.AddHours(1));
        unchanged.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeReference_and_notes_update_values_and_same_value_is_a_noop()
    {
        var payment = NewPayment();
        var reference = PaymentReference.Create("new reference");
        var notes = PaymentNotes.Create("new notes");

        payment.ChangeReference(reference, Actor, Created.AddHours(1));
        payment.ChangeNotes(notes, Actor, Created.AddHours(2));
        payment.Reference.Should().Be(reference);
        payment.Notes.Should().Be(notes);
        payment.UpdatedAtUtc.Should().Be(Created.AddHours(2));

        var unchanged = NewPayment();
        unchanged.ChangeReference(PaymentReference.CreateOptional(" ref "), Actor, Created.AddHours(1));
        unchanged.ChangeNotes(PaymentNotes.CreateOptional(" notes "), Actor, Created.AddHours(1));
        unchanged.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeMethods_reject_empty_actor_and_invalid_timestamps()
    {
        var payment = NewPayment();
        Action missingActor = () => payment.ChangeNotes(null, Guid.Empty, Created.AddHours(1));
        missingActor.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedByRequired);

        Action invalidTime = () => payment.ChangeReference(null, Actor, Created.AddMinutes(-1));
        invalidTime.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcInvalid);

        Action invalidMethod = () => payment.ChangeMethod((PaymentMethod)999, Actor, Created.AddHours(1));
        invalidMethod.Should().Throw<DomainException>().WithDomainError(DomainErrors.MethodInvalid);
    }

    [Fact]
    public void Post_validates_number_date_actor_and_timestamp()
    {
        var missingNumber = NewPayment();
        Action nullNumber = () => missingNumber.Post(null!, new DateOnly(2025, 1, 2), Actor, Created.AddHours(1));
        nullNumber.Should().Throw<ArgumentNullException>();

        var missingDate = NewPayment();
        Action invalidDate = () => missingDate.Post(PaymentNumber.Create("PAY-2025-1"), default, Actor, Created.AddHours(1));
        invalidDate.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentDateRequired);

        var missingActor = NewPayment();
        Action emptyActor = () => missingActor.Post(PaymentNumber.Create("PAY-2025-1"), new DateOnly(2025, 1, 2), Guid.Empty, Created.AddHours(1));
        emptyActor.Should().Throw<DomainException>().WithDomainError(DomainErrors.PostedByRequired);

        var missingTime = NewPayment();
        Action defaultTime = () => missingTime.Post(PaymentNumber.Create("PAY-2025-1"), new DateOnly(2025, 1, 2), Actor, default);
        defaultTime.Should().Throw<DomainException>().WithDomainError(DomainErrors.PostedAtUtcRequired);
    }

    [Fact]
    public void AddAllocation_rejects_unpaid_invoice_overallocation_and_missing_actor()
    {
        var payment = NewPayment(200m);
        var balance = NewBalance(payment, 100m);
        Post(payment);

        Action overInvoice = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(101m, Ron), Actor, Created.AddHours(2));
        overInvoice.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceOverAllocation);

        Action missingActor = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(10m, Ron), Guid.Empty, Created.AddHours(2));
        missingActor.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedByRequired);
    }

    [Fact]
    public void Reversed_allocation_can_be_reallocated_to_the_same_invoice()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        Post(payment);
        var originalId = Guid.CreateVersion7();
        payment.AddAllocation(originalId, balance, Money.Create(40m, Ron), Actor, Created.AddHours(2));

        Action duplicate = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(10m, Ron), Actor, Created.AddHours(3));
        duplicate.Should().Throw<DomainException>().WithDomainError(DomainErrors.ActiveAllocationForInvoiceExists);

        payment.ReverseAllocation(originalId, balance, Actor, "Correction", Created.AddHours(3));

        payment.HasAllocationForInvoice(balance.InvoiceId).Should().BeFalse();
        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(50m, Ron), Actor, Created.AddHours(4));
        payment.Allocations.Should().HaveCount(2);
        payment.AllocatedAmount.Amount.Should().Be(50m);
    }

    [Fact]
    public void ReverseAllocation_rejects_missing_actor_reason_and_null_balance()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(10m, Ron), Actor, Created.AddHours(2));

        Action missingActor = () => payment.ReverseAllocation(allocationId, balance, Guid.Empty, "Correction", Created.AddHours(3));
        missingActor.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationReversedByRequired);

        Action missingReason = () => payment.ReverseAllocation(allocationId, balance, Actor, " ", Created.AddHours(3));
        missingReason.Should().Throw<DomainException>().WithDomainError(DomainErrors.ReversalReasonRequired);

        Action nullBalance = () => payment.ReverseAllocation(allocationId, null!, Actor, "Correction", Created.AddHours(3));
        nullBalance.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAllocation_rejects_empty_and_unknown_ids()
    {
        var payment = NewPayment();
        Action empty = () => payment.GetAllocation(Guid.Empty);
        empty.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationIdInvalid);

        Action unknown = () => payment.GetAllocation(Guid.CreateVersion7());
        unknown.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationNotFound);
    }

    [Fact]
    public void AddAllocation_rejects_balance_identity_and_currency_mismatches()
    {
        var payment = NewPayment(200m);
        Post(payment);
        var currencyBalance = InvoicePaymentBalance.Create(payment.TenantId, Guid.CreateVersion7(), payment.CustomerId,
            InvoiceNumber.Create("INV-1"), CurrencyCode.Create("EUR"), Money.Create(100m, CurrencyCode.Create("EUR")), Created);
        Action currency = () => payment.AddAllocation(Guid.CreateVersion7(), currencyBalance, Money.Create(10m, Ron), Actor, Created.AddHours(2));
        currency.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceCurrencyMismatch);

        var tenantBalance = InvoicePaymentBalance.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), payment.CustomerId,
            InvoiceNumber.Create("INV-2"), Ron, Money.Create(100m, Ron), Created);
        Action tenant = () => payment.AddAllocation(Guid.CreateVersion7(), tenantBalance, Money.Create(10m, Ron), Actor, Created.AddHours(2));
        tenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceIdentityMismatch);

        var customerBalance = InvoicePaymentBalance.Create(payment.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(),
            InvoiceNumber.Create("INV-3"), Ron, Money.Create(100m, Ron), Created);
        Action customer = () => payment.AddAllocation(Guid.CreateVersion7(), customerBalance, Money.Create(10m, Ron), Actor, Created.AddHours(2));
        customer.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceIdentityMismatch);
    }
}
