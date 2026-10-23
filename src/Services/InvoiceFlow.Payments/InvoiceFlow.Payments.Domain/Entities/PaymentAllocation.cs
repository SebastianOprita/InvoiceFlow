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
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    public static PaymentAllocation Create(
        Guid tenantId,
        Guid id,
        Guid paymentId,
        Guid invoiceId,
        InvoiceNumber invoiceNumber,
        Money amount,
        DateTimeOffset createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidatePaymentId(paymentId);
        ValidateInvoiceId(invoiceId);
        ValidateAmount(amount);
        ValidateUtcTimestamp(createdAtUtc);

        return new PaymentAllocation(
            tenantId,
            id,
            paymentId,
            invoiceId,
            invoiceNumber,
            amount,
            createdAtUtc.ToUniversalTime());
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
}
