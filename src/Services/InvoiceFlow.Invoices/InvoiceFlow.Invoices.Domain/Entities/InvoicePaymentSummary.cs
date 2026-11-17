using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed class InvoicePaymentSummary
{
    public Guid TenantId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Money PaidAmount { get; private set; }
    public Money OutstandingAmount { get; private set; }
    public InvoicePaymentStatus Status { get; private set; }
    public long PaymentBalanceVersion { get; private set; }
    public DateTimeOffset SourceUpdatedAtUtc { get; private set; }
    public DateTimeOffset LastAppliedAtUtc { get; private set; }

#pragma warning disable CS8618
    private InvoicePaymentSummary() { }
#pragma warning restore CS8618

    private InvoicePaymentSummary(
        Guid tenantId,
        Guid invoiceId,
        Money paidAmount,
        Money outstandingAmount,
        InvoicePaymentStatus status,
        long paymentBalanceVersion,
        DateTimeOffset sourceUpdatedAtUtc,
        DateTimeOffset appliedAtUtc)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        PaidAmount = CopyMoney(paidAmount);
        OutstandingAmount = CopyMoney(outstandingAmount);
        Status = status;
        PaymentBalanceVersion = paymentBalanceVersion;
        SourceUpdatedAtUtc = sourceUpdatedAtUtc.ToUniversalTime();
        LastAppliedAtUtc = appliedAtUtc.ToUniversalTime();
    }

    public static InvoicePaymentSummary Create(
        Guid tenantId,
        Guid invoiceId,
        Money paidAmount,
        Money outstandingAmount,
        long paymentBalanceVersion,
        DateTimeOffset sourceUpdatedAtUtc,
        DateTimeOffset appliedAtUtc)
    {
        Validate(
            tenantId,
            invoiceId,
            paidAmount,
            outstandingAmount,
            paymentBalanceVersion,
            sourceUpdatedAtUtc,
            appliedAtUtc);

        var status = CalculateStatus(
            paidAmount,
            outstandingAmount);

        return new InvoicePaymentSummary(
            tenantId,
            invoiceId,
            paidAmount,
            outstandingAmount,
            status,
            paymentBalanceVersion,
            sourceUpdatedAtUtc,
            appliedAtUtc);
    }

    /// <summary>
    /// Applies a complete authoritative snapshot from Payments.
    /// Returns false when the snapshot is stale or already applied.
    /// </summary>
    public bool ApplySnapshot(
        Money paidAmount,
        Money outstandingAmount,
        long paymentBalanceVersion,
        DateTimeOffset sourceUpdatedAtUtc,
        DateTimeOffset appliedAtUtc)
    {
        if (paymentBalanceVersion <= PaymentBalanceVersion)
            return false;

        ValidateAmounts(paidAmount, outstandingAmount);

        if (paidAmount.CurrencyCode != PaidAmount.CurrencyCode)
            throw new DomainException(
                DomainErrors.CurrencyMismatch);

        if (sourceUpdatedAtUtc == default ||
            appliedAtUtc == default)
            throw new DomainException(
                DomainErrors.UpdatedAtUtcRequired);

        if (sourceUpdatedAtUtc.ToUniversalTime() <
            SourceUpdatedAtUtc)
        {
            throw new DomainException(
                DomainErrors.PaymentSummaryTimestampInvalid);
        }

        PaidAmount = CopyMoney(paidAmount);
        OutstandingAmount = CopyMoney(outstandingAmount);

        Status = CalculateStatus(
            paidAmount,
            outstandingAmount);

        PaymentBalanceVersion = paymentBalanceVersion;

        SourceUpdatedAtUtc = sourceUpdatedAtUtc.ToUniversalTime();
        LastAppliedAtUtc = appliedAtUtc.ToUniversalTime();

        return true;
    }

    private static InvoicePaymentStatus CalculateStatus(
        Money paidAmount,
        Money outstandingAmount)
    {
        if (outstandingAmount.Amount == 0m)
            return InvoicePaymentStatus.Paid;

        if (paidAmount.Amount == 0m)
            return InvoicePaymentStatus.Unpaid;

        return InvoicePaymentStatus.PartiallyPaid;
    }

    private static void Validate(
        Guid tenantId,
        Guid invoiceId,
        Money paidAmount,
        Money outstandingAmount,
        long paymentBalanceVersion,
        DateTimeOffset sourceUpdatedAtUtc,
        DateTimeOffset appliedAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(
                DomainErrors.TenantIdRequired);

        if (invoiceId == Guid.Empty)
            throw new DomainException(
                DomainErrors.IdRequired);

        if (paymentBalanceVersion < 0)
            throw new DomainException(
                DomainErrors.PaymentBalanceVersionInvalid);

        if (sourceUpdatedAtUtc == default ||
            appliedAtUtc == default)
            throw new DomainException(
                DomainErrors.UpdatedAtUtcRequired);

        ValidateAmounts(paidAmount, outstandingAmount);
    }

    private static void ValidateAmounts(
        Money paidAmount,
        Money outstandingAmount)
    {
        if (paidAmount.Amount < 0m ||
            outstandingAmount.Amount < 0m)
        {
            throw new DomainException(
                DomainErrors.PaymentSummaryAmountInvalid);
        }

        if (paidAmount.CurrencyCode !=
            outstandingAmount.CurrencyCode)
        {
            throw new DomainException(
                DomainErrors.CurrencyMismatch);
        }
    }

    private static Money CopyMoney(Money money)
    {
        return Money.Create(money.Amount, money.CurrencyCode);
    }
}
