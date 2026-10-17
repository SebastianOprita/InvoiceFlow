using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed class Payment
{
    private readonly List<PaymentAllocation> _allocations = [];
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public PaymentNumber Number { get; private set; }
    public Guid CustomerId { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public Money Amount { get; private set; }
    public Money AllocatedAmount => Money.Create(_allocations.Sum(x => x.Amount.Amount), CurrencyCode);
    public Money UnallocatedAmount => Money.Create(Amount.Amount - AllocatedAmount.Amount, CurrencyCode);
    public PaymentMethod Method { get; private set; }
    public PaymentReference? Reference { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public PaymentLifecycleStatus Status { get; private set; }
    public PaymentNotes? Notes { get; private set; }
    public DateTime? PostedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();

#pragma warning disable CS8618
    private Payment() { }
#pragma warning restore CS8618

    private Payment(
        Guid tenantId,
        Guid id,
        PaymentNumber number,
        Guid customerId,
        CurrencyCode currencyCode,
        Money amount,
        PaymentMethod method,
        PaymentReference? reference,
        DateOnly paymentDate,
        PaymentNotes? notes,
        DateTime createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Number = number;
        CustomerId = customerId;
        CurrencyCode = currencyCode;
        Amount = amount;
        Method = method;
        Reference = reference;
        PaymentDate = paymentDate;
        Notes = notes;

        Status = PaymentLifecycleStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    public static Payment Create(
        Guid tenantId,
        Guid id,
        PaymentNumber number,
        Guid customerId,
        CurrencyCode currencyCode,
        Money amount,
        PaymentMethod method,
        PaymentReference? reference,
        DateOnly paymentDate,
        PaymentNotes? notes,
        DateTime createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateCustomerId(customerId);
        ValidateCurrencyCode(currencyCode);
        ValidateAmount(amount);
        ValidateCurrencyMatches(currencyCode, amount);
        ValidateMethod(method);
        ValidatePaymentDate(paymentDate);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcInvalid);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new Payment(
            tenantId,
            id,
            number,
            customerId,
            currencyCode,
            amount,
            method,
            reference,
            paymentDate,
            notes,
            createdAtUtc);
    }

    public void ChangeNumber(
        PaymentNumber number,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (Number == number)
            return;

        Number = number;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeAmount(
        Money amount,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        ValidateAmount(amount);
        EnsureCurrencyMatches(amount);

        if (amount.Amount < AllocatedAmount.Amount)
            throw new DomainException(DomainErrors.AmountExceedsUnallocated);

        if (Amount == amount)
            return;

        Amount = amount;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangePaymentDetails(
        PaymentMethod method,
        PaymentReference? reference,
        DateOnly paymentDate,
        PaymentNotes? notes,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);
        ValidatePaymentDate(paymentDate);

        Method = method;
        Reference = reference;
        PaymentDate = paymentDate;
        Notes = notes;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeMethod(
        PaymentMethod method,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        ValidateMethod(method);

        if (Method == method)
            return;

        Method = method;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeReference(
        PaymentReference? reference,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (Reference == reference)
            return;

        Reference = reference;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangePaymentDate(
        DateOnly paymentDate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        ValidatePaymentDate(paymentDate);

        if (PaymentDate == paymentDate)
            return;

        PaymentDate = paymentDate;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeNotes(
        PaymentNotes? notes,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (Notes == notes)
            return;

        Notes = notes;

        MarkAsUpdated(updatedAtUtc);
    }

    public void AddAllocation(
        Guid allocationId,
        Guid invoiceId,
        InvoiceNumber invoiceNumber,
        Money amount,
        DateTime createdAtUtc)
    {
        EnsureDraft();

        ValidateAllocationId(allocationId);
        ValidateInvoiceId(invoiceId);
        ValidateAllocationAmount(amount);
        EnsureCurrencyMatches(amount);

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
            createdAtUtc);

        _allocations.Add(allocation);

        MarkAsUpdated(createdAtUtc);
    }

    public void RemoveAllocation(
        Guid allocationId,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        var allocation = GetAllocation(allocationId);

        _allocations.Remove(allocation);

        MarkAsUpdated(updatedAtUtc);
    }

    public void ReplaceAllocation(
        Guid allocationId,
        Guid newAllocationId,
        Money amount,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        var existingAllocation = GetAllocation(allocationId);

        ValidateAllocationId(newAllocationId);
        ValidateAllocationAmount(amount);
        EnsureCurrencyMatches(amount);

        var availableAmount =
            UnallocatedAmount.Amount +
            existingAllocation.Amount.Amount;

        if (amount.Amount > availableAmount)
            throw new DomainException(DomainErrors.AmountExceedsUnallocated);

        if (newAllocationId != allocationId && _allocations.Any(x => x.Id == newAllocationId))
            throw new DomainException(DomainErrors.AllocationIdInvalid);

        _allocations.Remove(existingAllocation);

        var replacement = PaymentAllocation.Create(
            TenantId,
            newAllocationId,
            Id,
            existingAllocation.InvoiceId,
            existingAllocation.InvoiceNumber,
            amount,
            updatedAtUtc);

        _allocations.Add(replacement);

        MarkAsUpdated(updatedAtUtc);
    }

    public void Post(DateTime postedAtUtc)
    {
        EnsureDraft();

        if (postedAtUtc == default)
            throw new DomainException(DomainErrors.PostedAtUtcRequired);

        if (postedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.PostedAtUtcNotUtc);

        if (_allocations.Count == 0)
            throw new DomainException(DomainErrors.NoAllocations);

        if (AllocatedAmount.Amount <= 0m)
            throw new DomainException(DomainErrors.AllocatedAmountInvalid);

        if (!IsFullyAllocated())
            throw new DomainException(DomainErrors.NotFullyAllocated);

        Status = PaymentLifecycleStatus.Posted;
        PostedAtUtc = postedAtUtc;

        MarkAsUpdated(postedAtUtc);
    }

    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status != PaymentLifecycleStatus.Posted)
            throw new DomainException(DomainErrors.CanNotBeCancelled);

        if (cancelledAtUtc == default)
            throw new DomainException(DomainErrors.CancelledAtUtcRequired);

        if (cancelledAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CancelledAtUtcNotUtc);

        Status = PaymentLifecycleStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;

        MarkAsUpdated(cancelledAtUtc);
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

    private void MarkAsUpdated(DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        if (updatedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.UpdatedAtUtcNotUtc);

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        if (UpdatedAtUtc.HasValue && updatedAtUtc < UpdatedAtUtc.Value)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        UpdatedAtUtc = updatedAtUtc;
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
