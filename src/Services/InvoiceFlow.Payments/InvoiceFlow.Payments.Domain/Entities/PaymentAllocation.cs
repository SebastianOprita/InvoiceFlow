using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed class PaymentAllocation
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public InvoiceNumber InvoiceNumber { get; private set; }
    public Money Amount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? ReversedAtUtc { get; private set; }
    public Guid? ReversedBy { get; private set; }
    public ReversalReason? ReversalReason { get; private set; }
    public bool IsReversed => ReversedAtUtc.HasValue;

#pragma warning disable CS8618
    private PaymentAllocation() { } // EF Core
#pragma warning restore CS8618

    private PaymentAllocation(
        Guid tenantId,
        Guid id,
        Guid paymentId,
        Guid invoiceId,
        InvoiceNumber invoiceNumber,
        Money amount,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        Amount = Money.Create(amount.Amount, amount.CurrencyCode);
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
    }

    public static PaymentAllocation Create(
        Guid tenantId,
        Guid id,
        Guid paymentId,
        Guid invoiceId,
        InvoiceNumber invoiceNumber,
        Money amount,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidatePaymentId(paymentId);
        ValidateInvoiceId(invoiceId);
        ValidateAmount(amount);
        ValidateUtcTimestamp(createdAtUtc);
        ValidateCreatedBy(createdBy);

        return new PaymentAllocation(
            tenantId,
            id,
            paymentId,
            invoiceId,
            invoiceNumber,
            amount,
            createdBy,
            createdAtUtc.ToUniversalTime());
    }

    internal void Reverse(
        Guid customerId,
        InvoicePaymentBalance invoiceBalance,
        Guid reversedBy,
        string reason,
        DateTimeOffset reversedAtUtc)
    {
        if (IsReversed)
            throw new DomainException(DomainErrors.AllocationAlreadyReversed);

        ValidateReversedBy(reversedBy);

        var normalizedReason = InvoiceFlow.Payments.Domain.ReversalReason.Create(reason);
        if (reversedAtUtc == default || reversedAtUtc.ToUniversalTime() < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);
        var utc = reversedAtUtc.ToUniversalTime();

        invoiceBalance.ReverseAllocation(customerId, this, utc);

        ReversedAtUtc = utc;
        ReversedBy = reversedBy;
        ReversalReason = normalizedReason;
    }

    private static void ValidateTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);
    }

    private static void ValidatePaymentId(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
            throw new DomainException(DomainErrors.PaymentIdRequired);
    }

    private static void ValidateInvoiceId(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new DomainException(DomainErrors.InvoiceIdRequired);
    }

    private static void ValidateAmount(Money amount)
    {
        if (amount.Amount <= 0m)
            throw new DomainException(DomainErrors.AllocationAmountInvalid);
    }

    private static void ValidateUtcTimestamp(DateTimeOffset createdAtUtc)
    {
        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcInvalid);
    }

    private static void ValidateCreatedBy(Guid createdBy)
    {
        if (createdBy == Guid.Empty)
            throw new DomainException(DomainErrors.CreatedByRequired);
    }

    private static void ValidateReversedBy(Guid reversedBy)
    {
        if (reversedBy == Guid.Empty)
            throw new DomainException(DomainErrors.AllocationReversedByRequired);
    }
}
