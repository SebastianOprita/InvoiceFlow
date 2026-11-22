using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed class Payment
{
    private readonly List<PaymentAllocation> _allocations = [];
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public PaymentNumber? Number { get; private set; }
    public Guid CustomerId { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public Money Amount { get; private set; }
    public Money AllocatedAmount =>
        Money.Create(
            _allocations.Where(x => !x.IsReversed).Sum(x => x.Amount.Amount),
            CurrencyCode);
    public Money UnallocatedAmount => Money.Create(Amount.Amount - AllocatedAmount.Amount, CurrencyCode);
    public PaymentMethod Method { get; private set; }
    public PaymentReference? Reference { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public PaymentLifecycleStatus Status { get; private set; }
    public PaymentNotes? Notes { get; private set; }
    public Guid? PostedBy { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();

#pragma warning disable CS8618
    private Payment() { }
#pragma warning restore CS8618

    private Payment(
        Guid tenantId,
        Guid id,
        Guid customerId,
        CurrencyCode currencyCode,
        Money amount,
        PaymentMethod method,
        PaymentReference? reference,
        PaymentNotes? notes,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Number = null;
        CustomerId = customerId;
        CurrencyCode = currencyCode;
        Amount = CopyMoney(amount);
        Method = method;
        Reference = reference;
        Notes = notes;

        Status = PaymentLifecycleStatus.Draft;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
    }

    public static Payment Create(
        Guid tenantId,
        Guid id,
        Guid customerId,
        CurrencyCode currencyCode,
        Money amount,
        PaymentMethod method,
        PaymentReference? reference,
        PaymentNotes? notes,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateCustomerId(customerId);
        ValidateCurrencyCode(currencyCode);
        ValidateAmount(amount);
        ValidateCurrencyMatches(currencyCode, amount);
        ValidateMethod(method);
        ValidateCreatedBy(createdBy);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcInvalid);

        return new Payment(
            tenantId,
            id,
            customerId,
            currencyCode,
            amount,
            method,
            reference,
            notes,
            createdBy,
            createdAtUtc.ToUniversalTime());
    }

    public void ChangeAmount(
        Money amount,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();

        ValidateAmount(amount);
        EnsureCurrencyMatches(amount);
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        if (amount.Amount < AllocatedAmount.Amount)
            throw new DomainException(DomainErrors.PaymentAmountBelowAllocated);

        if (Amount == amount)
            return;

        Amount = CopyMoney(amount);

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangePaymentDetails(
        PaymentMethod method,
        PaymentReference? reference,
        PaymentNotes? notes,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        Method = method;
        Reference = reference;
        Notes = notes;

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeMethod(
        PaymentMethod method,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        if (Method == method)
            return;

        Method = method;

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeReference(
        PaymentReference? reference,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        if (Reference == reference)
            return;

        Reference = reference;

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeNotes(
        PaymentNotes? notes,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        if (Notes == notes)
            return;

        Notes = notes;

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void AddAllocation(
        Guid allocationId,
        InvoicePaymentBalance invoiceBalance,
        Money amount,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsurePosted();

        ValidateAllocationId(allocationId);
        ValidateAllocationAmount(amount);
        EnsureCurrencyMatches(amount);
        ValidateUpdatedAtUtc(updatedAtUtc);
        ValidateUpdatedBy(updatedBy);

        ArgumentNullException.ThrowIfNull(invoiceBalance);

        if (_allocations.Any(x => x.Id == allocationId))
            throw new DomainException(DomainErrors.AllocationIdInvalid);

        if (HasAllocationForInvoice(invoiceBalance.InvoiceId))
            throw new DomainException(DomainErrors.ActiveAllocationForInvoiceExists);

        if (amount.Amount > UnallocatedAmount.Amount)
            throw new DomainException(DomainErrors.AmountExceedsUnallocated);

        var allocation = PaymentAllocation.Create(
            TenantId,
            allocationId,
            Id,
            invoiceBalance.InvoiceId,
            invoiceBalance.InvoiceNumber,
            amount,
            updatedBy,
            updatedAtUtc);

        invoiceBalance.Allocate(CustomerId, allocation, updatedAtUtc);
        _allocations.Add(allocation);

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ReverseAllocation(
        Guid allocationId,
        InvoicePaymentBalance invoiceBalance,
        Guid reversedBy,
        string reason,
        DateTimeOffset reversedAtUtc)
    {
        EnsurePosted();
        ArgumentNullException.ThrowIfNull(invoiceBalance);
        ValidateUpdatedAtUtc(reversedAtUtc);
        ValidateReversedBy(reversedBy);

        var allocation = GetAllocation(allocationId);
        if (allocation.PaymentId != Id || allocation.TenantId != TenantId)
            throw new DomainException(DomainErrors.AllocationPaymentMismatch);

        allocation.Reverse(CustomerId, invoiceBalance, reversedBy, reason, reversedAtUtc);

        UpdatedBy = reversedBy;
        UpdatedAtUtc = reversedAtUtc.ToUniversalTime();
    }

    public void Post(PaymentNumber number, DateOnly paymentDate, Guid postedBy, DateTimeOffset postedAtUtc)
    {
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(number);
        ValidatePaymentDate(paymentDate);

        if (postedAtUtc == default)
            throw new DomainException(DomainErrors.PostedAtUtcRequired);

        ValidatePostedBy(postedBy);
        postedAtUtc = postedAtUtc.ToUniversalTime();

        if (_allocations.Count != 0)
            throw new DomainException(DomainErrors.PaymentHasAllocations);

        Number = number;
        PaymentDate = paymentDate;
        Status = PaymentLifecycleStatus.Posted;

        PostedBy = postedBy;
        PostedAtUtc = postedAtUtc;
        UpdatedBy = postedBy;
        UpdatedAtUtc = postedAtUtc;
    }

    public bool IsFullyAllocated()
    {
        return UnallocatedAmount.Amount == 0m;
    }

    public bool HasAllocationForInvoice(Guid invoiceId)
    {
        return _allocations.Any(x => x.InvoiceId == invoiceId && !x.IsReversed);
    }

    public PaymentAllocation GetAllocation(Guid allocationId)
    {
        ValidateAllocationId(allocationId);

        return _allocations.FirstOrDefault(x => x.Id == allocationId)
            ?? throw new DomainException(DomainErrors.AllocationNotFound);
    }

    private void EnsureDraft()
    {
        if (Status != PaymentLifecycleStatus.Draft)
            throw new DomainException(DomainErrors.CanNotBeModified);
    }

    private void EnsurePosted()
    {
        if (Status != PaymentLifecycleStatus.Posted)
            throw new DomainException(DomainErrors.PaymentMustBePosted);
    }

    private void EnsureCurrencyMatches(Money money)
    {
        ValidateCurrencyMatches(CurrencyCode, money);
    }

    private void ValidateUpdatedAtUtc(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        updatedAtUtc = updatedAtUtc.ToUniversalTime();

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        if (UpdatedAtUtc.HasValue && updatedAtUtc < UpdatedAtUtc.Value)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);
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

    private static void ValidateCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrors.CustomerIdRequired);
    }

    private static void ValidateCurrencyCode(CurrencyCode currencyCode)
    {
        if (currencyCode is null)
            throw new DomainException(DomainErrors.CurrencyCodeRequired);
    }

    private static void ValidateAmount(Money amount)
    {
        if (amount is null || amount.Amount <= 0m)
            throw new DomainException(DomainErrors.AmountInvalid);
    }

    private static Money CopyMoney(Money money)
    {
        return Money.Create(money.Amount, money.CurrencyCode);
    }

    private static void ValidateAllocationAmount(Money amount)
    {
        if (amount is null || amount.Amount <= 0m)
            throw new DomainException(DomainErrors.AllocationAmountInvalid);
    }

    private static void ValidateCurrencyMatches(CurrencyCode currencyCode, Money amount)
    {
        if (amount.CurrencyCode != currencyCode)
            throw new DomainException(DomainErrors.CurrencyMismatch);
    }

    private static void ValidateAllocationId(Guid allocationId)
    {
        if (allocationId == Guid.Empty)
            throw new DomainException(DomainErrors.AllocationIdInvalid);
    }

    private static void ValidateMethod(PaymentMethod method)
    {
        if (!Enum.IsDefined(method))
            throw new DomainException(DomainErrors.MethodInvalid);
    }

    private static void ValidatePaymentDate(DateOnly paymentDate)
    {
        if (paymentDate == default)
            throw new DomainException(DomainErrors.PaymentDateRequired);
    }

    private static void ValidateCreatedBy(Guid createdBy)
    {
        if (createdBy == Guid.Empty)
            throw new DomainException(DomainErrors.CreatedByRequired);
    }

    private static void ValidateUpdatedBy(Guid updatedBy)
    {
        if (updatedBy == Guid.Empty)
            throw new DomainException(DomainErrors.UpdatedByRequired);
    }

    private static void ValidatePostedBy(Guid postedBy)
    {
        if (postedBy == Guid.Empty)
            throw new DomainException(DomainErrors.PostedByRequired);
    }

    private static void ValidateReversedBy(Guid reversedBy)
    {
        if (reversedBy == Guid.Empty)
            throw new DomainException(DomainErrors.AllocationReversedByRequired);
    }
}
