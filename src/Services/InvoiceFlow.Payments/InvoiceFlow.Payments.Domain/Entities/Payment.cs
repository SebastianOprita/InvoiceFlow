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
    public Money AllocatedAmount => Money.Create(_allocations.Sum(x => x.Amount.Amount), CurrencyCode);
    public Money UnallocatedAmount => Money.Create(Amount.Amount - AllocatedAmount.Amount, CurrencyCode);
    public PaymentMethod Method { get; private set; }
    public PaymentReference? Reference { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public PaymentLifecycleStatus Status { get; private set; }
    public PaymentNotes? Notes { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
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
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Number = null;
        CustomerId = customerId;
        CurrencyCode = currencyCode;
        Amount = amount;
        Method = method;
        Reference = reference;
        Notes = notes;

        Status = PaymentLifecycleStatus.Draft;
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
        DateTimeOffset createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateCustomerId(customerId);
        ValidateCurrencyCode(currencyCode);
        ValidateAmount(amount);
        ValidateCurrencyMatches(currencyCode, amount);
        ValidateMethod(method);

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
            createdAtUtc.ToUniversalTime());
    }

    public void ChangeAmount(
        Money amount,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();

        ValidateAmount(amount);
        EnsureCurrencyMatches(amount);
        ValidateUpdatedAtUtc(updatedAtUtc);

        if (amount.Amount < AllocatedAmount.Amount)
            throw new DomainException(DomainErrors.PaymentAmountBelowAllocated);

        if (Amount == amount)
            return;

        Amount = amount;

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangePaymentDetails(
        PaymentMethod method,
        PaymentReference? reference,
        PaymentNotes? notes,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);
        ValidateUpdatedAtUtc(updatedAtUtc);

        Method = method;
        Reference = reference;
        Notes = notes;

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeMethod(
        PaymentMethod method,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);
        ValidateUpdatedAtUtc(updatedAtUtc);

        if (Method == method)
            return;

        Method = method;

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeReference(
        PaymentReference? reference,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateUpdatedAtUtc(updatedAtUtc);

        if (Reference == reference)
            return;

        Reference = reference;

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void ChangeNotes(
        PaymentNotes? notes,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateUpdatedAtUtc(updatedAtUtc);

        if (Notes == notes)
            return;

        Notes = notes;

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void AddAllocation(
        Guid allocationId,
        Guid invoiceId,
        InvoiceNumber invoiceNumber,
        Money amount,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();

        ValidateAllocationId(allocationId);
        ValidateInvoiceId(invoiceId);
        ValidateAllocationAmount(amount);
        EnsureCurrencyMatches(amount);
        ValidateUpdatedAtUtc(updatedAtUtc);

        if (_allocations.Any(x => x.Id == allocationId))
            throw new DomainException(DomainErrors.AllocationIdInvalid);

        if (_allocations.Any(x => x.InvoiceId == invoiceId))
            throw new DomainException(DomainErrors.InvoiceIdInvalid);

        if (amount.Amount > UnallocatedAmount.Amount)
            throw new DomainException(DomainErrors.AmountExceedsUnallocated);

        var allocation = PaymentAllocation.Create(
            TenantId,
            allocationId,
            Id,
            invoiceId,
            invoiceNumber,
            amount,
            updatedAtUtc);

        _allocations.Add(allocation);

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void RemoveAllocation(
        Guid allocationId,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraft();
        ValidateUpdatedAtUtc(updatedAtUtc);

        var allocation = GetAllocation(allocationId);

        _allocations.Remove(allocation);

        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public void Post(PaymentNumber number, DateOnly paymentDate, DateTimeOffset postedAtUtc)
    {
        EnsureDraft();
        ValidatePaymentDate(paymentDate);
        ValidateUpdatedAtUtc(postedAtUtc);

        if (postedAtUtc == default)
            throw new DomainException(DomainErrors.PostedAtUtcRequired);

        postedAtUtc = postedAtUtc.ToUniversalTime();

        if (_allocations.Count == 0)
            throw new DomainException(DomainErrors.NoAllocations);

        if (AllocatedAmount.Amount <= 0m)
            throw new DomainException(DomainErrors.AllocatedAmountInvalid);

        if (!IsFullyAllocated())
            throw new DomainException(DomainErrors.NotFullyAllocated);

        Number = number;
        PaymentDate = paymentDate;
        Status = PaymentLifecycleStatus.Posted;
        PostedAtUtc = postedAtUtc;

        UpdatedAtUtc = postedAtUtc.ToUniversalTime();
    }

    public bool IsFullyAllocated()
    {
        return UnallocatedAmount.Amount == 0m;
    }

    public bool HasAllocationForInvoice(Guid invoiceId)
    {
        return _allocations.Any(x => x.InvoiceId == invoiceId);
    }

    private PaymentAllocation GetAllocation(Guid allocationId)
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

    private void EnsureCurrencyMatches(Money money)
    {
        ValidateCurrencyMatches(CurrencyCode, money);
    }

    private void ValidateUpdatedAtUtc(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

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

    private static void ValidateInvoiceId(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new DomainException(DomainErrors.InvoiceIdInvalid);
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
}
