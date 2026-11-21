using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed class InvoicePaymentBalance
{
    public Guid TenantId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }

    public InvoiceNumber InvoiceNumber { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }

    public Money TotalAmount { get; private set; }
    public Money AllocatedAmount { get; private set; }

    public Money OutstandingAmount =>
        Money.Create(
            TotalAmount.Amount - AllocatedAmount.Amount,
            CurrencyCode);

    public InvoicePaymentStatus PaymentStatus =>
        AllocatedAmount.Amount switch
        {
            0m => InvoicePaymentStatus.Unpaid,

            var allocated when allocated < TotalAmount.Amount =>
                InvoicePaymentStatus.PartiallyPaid,

            _ => InvoicePaymentStatus.Paid
        };

    public long Version { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private InvoicePaymentBalance() { }
#pragma warning restore CS8618

    private InvoicePaymentBalance(
        Guid tenantId,
        Guid invoiceId,
        Guid customerId,
        InvoiceNumber invoiceNumber,
        CurrencyCode currencyCode,
        Money totalAmount,
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        CustomerId = customerId;

        InvoiceNumber = invoiceNumber;
        CurrencyCode = currencyCode;

        TotalAmount = Money.Create(totalAmount.Amount, totalAmount.CurrencyCode);
        AllocatedAmount = Money.Zero(currencyCode);

        Version = 0;
        CreatedAtUtc = createdAtUtc;
    }

    public static InvoicePaymentBalance Create(
        Guid tenantId,
        Guid invoiceId,
        Guid customerId,
        InvoiceNumber invoiceNumber,
        CurrencyCode currencyCode,
        Money totalAmount,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (invoiceId == Guid.Empty)
            throw new DomainException(DomainErrors.InvoiceIdInvalid);

        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrors.CustomerIdRequired);

        ArgumentNullException.ThrowIfNull(invoiceNumber);
        ArgumentNullException.ThrowIfNull(currencyCode);

        if (totalAmount is null || totalAmount.Amount <= 0m)
            throw new DomainException(DomainErrors.AmountInvalid);

        if (totalAmount.CurrencyCode != currencyCode)
            throw new DomainException(DomainErrors.CurrencyMismatch);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcInvalid);

        return new InvoicePaymentBalance(
            tenantId,
            invoiceId,
            customerId,
            invoiceNumber,
            currencyCode,
            totalAmount,
            createdAtUtc.ToUniversalTime());
    }

    // Allocation
    // ------------------------------------------------------------

    public void Allocate(
        Money amount,
        DateTimeOffset updatedAtUtc)
    {
        ValidateAmount(amount);
        EnsureCurrencyMatches(amount);

        if (amount.Amount > OutstandingAmount.Amount)
            throw new DomainException(DomainErrors.InvoicePaymentBalanceOverAllocation);

        var utc = ValidateUpdatedAtUtc(updatedAtUtc);

        AllocatedAmount = Money.Create(
            AllocatedAmount.Amount + amount.Amount,
            CurrencyCode);

        Version++;
        UpdatedAtUtc = utc;
    }

    // Reversal
    // ------------------------------------------------------------

    public void ReverseAllocation(
        Money amount,
        DateTimeOffset updatedAtUtc)
    {
        ValidateAmount(amount);
        EnsureCurrencyMatches(amount);

        if (amount.Amount > AllocatedAmount.Amount)
            throw new DomainException(DomainErrors.InvoicePaymentBalanceOverReversal);

        var utc = ValidateUpdatedAtUtc(updatedAtUtc);

        AllocatedAmount = Money.Create(
            AllocatedAmount.Amount - amount.Amount,
            CurrencyCode);

        Version++;
        UpdatedAtUtc = utc;
    }

    // Helpers
    // ------------------------------------------------------------

    public void EnsureCanAllocate(
        Guid tenantId,
        Guid customerId,
        CurrencyCode currencyCode,
        Money amount)
    {
        ValidateAmount(amount);

        if (TenantId != tenantId || CustomerId != customerId)
            throw new DomainException(DomainErrors.InvoicePaymentBalanceIdentityMismatch);

        if (CurrencyCode != currencyCode || amount.CurrencyCode != CurrencyCode)
            throw new DomainException(DomainErrors.InvoicePaymentBalanceCurrencyMismatch);

        if (amount.Amount > OutstandingAmount.Amount)
            throw new DomainException(DomainErrors.InvoicePaymentBalanceOverAllocation);
    }

    private void EnsureCurrencyMatches(Money amount)
    {
        if (amount.CurrencyCode != CurrencyCode)
            throw new DomainException(DomainErrors.CurrencyMismatch);
    }

    private static void ValidateAmount(Money amount)
    {
        if (amount is null || amount.Amount <= 0m)
            throw new DomainException(DomainErrors.AllocationAmountInvalid);
    }

    private DateTimeOffset ValidateUpdatedAtUtc(
        DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        var utc = updatedAtUtc.ToUniversalTime();

        if (utc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        if (UpdatedAtUtc.HasValue && utc < UpdatedAtUtc.Value)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        return utc;
    }
}
