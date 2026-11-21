using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T10:00:00Z");
    private static readonly CurrencyCode Ron = CurrencyCode.Create("RON");

    private static Payment NewPayment(decimal amount = 100m) => Payment.Create(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Ron,
        Money.Create(amount, Ron), PaymentMethod.Card,
        PaymentReference.CreateOptional(" ref "), PaymentNotes.CreateOptional(" notes "), Created);

    private static InvoicePaymentBalance NewBalance(
        Payment payment,
        decimal amount = 100m,
        Guid? tenantId = null,
        Guid? customerId = null,
        CurrencyCode? currencyCode = null) => InvoicePaymentBalance.Create(
            tenantId ?? payment.TenantId,
            Guid.CreateVersion7(),
            customerId ?? payment.CustomerId,
            InvoiceNumber.Create("INV-1"),
            currencyCode ?? payment.CurrencyCode,
            Money.Create(amount, currencyCode ?? payment.CurrencyCode),
            Created);

    private static void Post(Payment payment, DateTimeOffset? at = null) =>
        payment.Post(PaymentNumber.Create("PAY-2025-1"), new DateOnly(2025, 1, 2), at ?? Created.AddHours(1));

    [Fact]
    public void Create_sets_draft_state_and_utc_timestamp()
    {
        var payment = NewPayment();
        payment.Status.Should().Be(PaymentLifecycleStatus.Draft);
        payment.Number.Should().BeNull();
        payment.AllocatedAmount.Amount.Should().Be(0m);
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.CreatedAtUtc.Should().Be(Created);
    }

    [Fact]
    public void Create_rejects_invalid_identity_amount_currency_and_method()
    {
        var tenant = Guid.CreateVersion7(); var id = Guid.CreateVersion7(); var customer = Guid.CreateVersion7();
        Action emptyTenant = () => Payment.Create(Guid.Empty, id, customer, Ron, Money.Create(1m, Ron), PaymentMethod.Card, null, null, Created);
        emptyTenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
        Action emptyCustomer = () => Payment.Create(tenant, id, Guid.Empty, Ron, Money.Create(1m, Ron), PaymentMethod.Card, null, null, Created);
        emptyCustomer.Should().Throw<DomainException>().WithDomainError(DomainErrors.CustomerIdRequired);
        Action zeroAmount = () => Payment.Create(tenant, id, customer, Ron, Money.Zero(Ron), PaymentMethod.Card, null, null, Created);
        zeroAmount.Should().Throw<DomainException>().WithDomainError(DomainErrors.AmountInvalid);
        Action currencyMismatch = () => Payment.Create(tenant, id, customer, Ron, Money.Create(1m, CurrencyCode.Create("EUR")), PaymentMethod.Card, null, null, Created);
        currencyMismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
        Action invalidMethod = () => Payment.Create(tenant, id, customer, Ron, Money.Create(1m, Ron), (PaymentMethod)99, null, null, Created);
        invalidMethod.Should().Throw<DomainException>().WithDomainError(DomainErrors.MethodInvalid);
    }

    [Fact]
    public void Post_allows_a_draft_payment_with_no_allocations_and_keeps_funds_unallocated()
    {
        var payment = NewPayment();
        Post(payment);

        payment.Status.Should().Be(PaymentLifecycleStatus.Posted);
        payment.Number!.Value.Should().Be("PAY-2025-1");
        payment.Allocations.Should().BeEmpty();
        payment.AllocatedAmount.Amount.Should().Be(0m);
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.IsFullyAllocated().Should().BeFalse();
    }

    [Fact]
    public void Post_requires_a_draft_payment()
    {
        var payment = NewPayment();
        Post(payment);

        Action act = () => Post(payment, Created.AddHours(2));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CanNotBeModified);
    }

    [Fact]
    public void Allocate_requires_payment_to_be_posted()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);

        Action act = () => payment.AddAllocation(
            Guid.CreateVersion7(), balance, Money.Create(10m, Ron), Created.AddHours(1));

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentMustBePosted);
    }

    [Fact]
    public void Allocate_updates_payment_and_invoice_balance_for_a_partial_payment()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var amount = Money.Create(60m, Ron);
        Post(payment);

        payment.AddAllocation(Guid.CreateVersion7(), balance, amount, Created.AddHours(2));
        balance.Allocate(amount, Created.AddHours(2));

        payment.AllocatedAmount.Amount.Should().Be(60m);
        payment.UnallocatedAmount.Amount.Should().Be(40m);
        payment.Allocations.Single().InvoiceId.Should().Be(balance.InvoiceId);
        balance.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        balance.OutstandingAmount.Amount.Should().Be(40m);
    }

    [Fact]
    public void Allocate_rejects_nonpositive_amount_and_payment_overallocation()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment, amount: 150m);
        Post(payment);

        Action zero = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Zero(Ron), Created.AddHours(2));
        zero.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationAmountInvalid);

        Action overPayment = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(101m, Ron), Created.AddHours(2));
        overPayment.Should().Throw<DomainException>().WithDomainError(DomainErrors.AmountExceedsUnallocated);
    }

    [Fact]
    public void Allocate_rejects_a_paid_invoice_or_amount_over_invoice_outstanding()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        balance.Allocate(Money.Create(100m, Ron), Created.AddMinutes(30));
        Post(payment);

        Action act = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(1m, Ron), Created.AddHours(2));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceOverAllocation);
    }

    [Fact]
    public void Allocate_rejects_currency_tenant_and_customer_mismatches()
    {
        var payment = NewPayment();
        Post(payment);

        var currencyBalance = NewBalance(payment, currencyCode: CurrencyCode.Create("EUR"));
        Action currency = () => payment.AddAllocation(Guid.CreateVersion7(), currencyBalance, Money.Create(10m, Ron), Created.AddHours(2));
        currency.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceCurrencyMismatch);

        var tenantBalance = NewBalance(payment, tenantId: Guid.CreateVersion7());
        Action tenant = () => payment.AddAllocation(Guid.CreateVersion7(), tenantBalance, Money.Create(10m, Ron), Created.AddHours(2));
        tenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceIdentityMismatch);

        var customerBalance = NewBalance(payment, customerId: Guid.CreateVersion7());
        Action customer = () => payment.AddAllocation(Guid.CreateVersion7(), customerBalance, Money.Create(10m, Ron), Created.AddHours(2));
        customer.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoicePaymentBalanceIdentityMismatch);
    }

    [Fact]
    public void Allocate_rejects_duplicate_invoice_and_currency_mismatch()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        Post(payment);
        payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(40m, Ron), Created.AddHours(2));

        Action duplicate = () => payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(10m, Ron), Created.AddHours(3));
        duplicate.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceIdInvalid);

        Action currency = () => payment.AddAllocation(Guid.CreateVersion7(), NewBalance(payment), Money.Create(1m, CurrencyCode.Create("EUR")), Created.AddHours(3));
        currency.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Fact]
    public void Allocate_rejects_duplicate_allocation_id_and_null_balance()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(10m, Ron), Created.AddHours(2));

        Action duplicate = () => payment.AddAllocation(allocationId, NewBalance(payment), Money.Create(10m, Ron), Created.AddHours(3));
        duplicate.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationIdInvalid);

        Action nullBalance = () => payment.AddAllocation(Guid.CreateVersion7(), null!, Money.Create(10m, Ron), Created.AddHours(3));
        nullBalance.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveAllocation_removes_posted_allocation_and_rejects_invalid_requests()
    {
        var payment = NewPayment();
        var balance = NewBalance(payment);
        var allocationId = Guid.CreateVersion7();
        Post(payment);
        payment.AddAllocation(allocationId, balance, Money.Create(40m, Ron), Created.AddHours(2));

        payment.RemoveAllocation(allocationId, Created.AddHours(3));

        payment.Allocations.Should().BeEmpty();
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.HasAllocationForInvoice(balance.InvoiceId).Should().BeFalse();

        Action missing = () => payment.RemoveAllocation(Guid.CreateVersion7(), Created.AddHours(4));
        missing.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationNotFound);

        Action invalidId = () => payment.RemoveAllocation(Guid.Empty, Created.AddHours(4));
        invalidId.Should().Throw<DomainException>().WithDomainError(DomainErrors.AllocationIdInvalid);
    }

    [Fact]
    public void RemoveAllocation_requires_posted_payment_and_monotonic_timestamp()
    {
        var payment = NewPayment();
        Action draft = () => payment.RemoveAllocation(Guid.CreateVersion7(), Created.AddHours(1));
        draft.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentMustBePosted);

        Post(payment);
        Action defaultTimestamp = () => payment.RemoveAllocation(Guid.CreateVersion7(), default);
        defaultTimestamp.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcRequired);

        Action beforeCreation = () => payment.RemoveAllocation(Guid.CreateVersion7(), Created.AddMinutes(-1));
        beforeCreation.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcInvalid);
    }

}
