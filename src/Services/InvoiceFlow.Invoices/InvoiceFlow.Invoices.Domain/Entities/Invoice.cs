using InvoiceFlow.BuildingBlocks.Domain;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Invoices.Domain;

public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];

    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public InvoiceNumber? Number { get; private set; }
    public Guid CustomerId { get; private set; }
    public CustomerCode CustomerCode { get; private set; }
    public CustomerName CustomerName { get; private set; }
    public CustomerEmail? CustomerEmail { get; private set; }
    public PhoneNumber? CustomerPhone { get; private set; }
    public TaxNumber? CustomerTaxNumber { get; private set; }
    public RegistrationNumber CustomerRegistrationNumber { get; private set; }
    public AddressLine1 CustomerAddressLine1 { get; private set; }
    public AddressLine2? CustomerAddressLine2 { get; private set; }
    public City CustomerCity { get; private set; }
    public State? CustomerState { get; private set; }
    public Country CustomerCountry { get; private set; }
    public PostalCode CustomerPostalCode { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public PaymentTermDays PaymentTermDays { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money NetAmount { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money GrossAmount { get; private set; }
    public Money PaidAmount { get; private set; }
    public Money RemainingAmount => Money.Create(GrossAmount.Amount - PaidAmount.Amount, CurrencyCode);

    public InvoicePaymentStatus PaymentStatus =>
        PaidAmount.Amount switch
        {
            0m => InvoicePaymentStatus.Unpaid,
            var paid when paid < GrossAmount.Amount =>
                InvoicePaymentStatus.PartiallyPaid,
            _ => InvoicePaymentStatus.Paid
        };

    public InvoiceNotes? Notes { get; private set; }
    public DateTime? IssuedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

#pragma warning disable CS8618
    private Invoice() { }
#pragma warning restore CS8618

    private Invoice(
        Guid tenantId,
        Guid id,
        InvoiceCustomerSnapshot customer,
        CurrencyCode currencyCode,
        PaymentTermDays paymentTermDays,
        DateOnly issueDate,
        DateOnly dueDate,
        InvoiceNotes? notes,
        DateTime createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Number = null;

        CopyCustomerSnapshot(customer);

        CurrencyCode = currencyCode;
        PaymentTermDays = paymentTermDays;
        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;

        Status = InvoiceStatus.Draft;

        DiscountAmount = Money.Zero(currencyCode);
        NetAmount = Money.Zero(currencyCode);
        TaxAmount = Money.Zero(currencyCode);
        GrossAmount = Money.Zero(currencyCode);
        PaidAmount = Money.Zero(currencyCode);

        CreatedAtUtc = createdAtUtc;
    }

    public static Invoice Create(
        Guid tenantId,
        Guid id,
        InvoiceCustomerSnapshot customer,
        CurrencyCode currencyCode,
        PaymentTermDays paymentTermDays,
        DateOnly issueDate,
        DateOnly dueDate,
        InvoiceNotes? notes,
        DateTime createdAtUtc)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateDates(issueDate, dueDate);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        if (customer.CustomerTenantId != tenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!customer.IsActive)
            throw new DomainException(DomainErrors.CustomerInactive);

        return new Invoice(
            tenantId,
            id,
            customer,
            currencyCode,
            paymentTermDays,
            issueDate,
            dueDate,
            notes,
            createdAtUtc);
    }

    // Header
    // -----------------------------------------------------------------

    public void ChangeCustomer(
        InvoiceCustomerSnapshot customer,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (customer.CustomerTenantId != TenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!customer.IsActive)
            throw new DomainException(DomainErrors.CustomerInactive);

        CopyCustomerSnapshot(customer);

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeDates(
        DateOnly issueDate,
        DateOnly dueDate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        ValidateDates(issueDate, dueDate);

        if (IssueDate == issueDate && DueDate == dueDate)
            return;

        IssueDate = issueDate;
        DueDate = dueDate;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangePaymentTerms(
        PaymentTermDays paymentTermDays,
        DateOnly dueDate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        ValidateDates(IssueDate, dueDate);

        PaymentTermDays = paymentTermDays;
        DueDate = dueDate;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeCurrency(
        CurrencyCode currencyCode,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (_lines.Count > 0)
            throw new DomainException(DomainErrors.CurrencyHasLines);

        if (CurrencyCode == currencyCode)
            return;

        CurrencyCode = currencyCode;

        DiscountAmount = Money.Zero(currencyCode);
        NetAmount = Money.Zero(currencyCode);
        TaxAmount = Money.Zero(currencyCode);
        GrossAmount = Money.Zero(currencyCode);
        PaidAmount = Money.Zero(currencyCode);

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeNotes(
        InvoiceNotes? notes,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (Notes == notes)
            return;

        Notes = notes;

        MarkAsUpdated(updatedAtUtc);
    }

    // Manual invoice lines
    // -----------------------------------------------------------------

    public void AddManualLine(
        Guid lineId,
        InvoiceLineDescription description,
        Quantity quantity,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage discountRate,
        Percentage taxRate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        EnsureUniqueLineId(lineId);
        EnsureCurrencyMatches(unitPrice);

        var line = InvoiceLine.CreateManual(
            TenantId,
            lineId,
            Id,
            GetNextPosition(),
            description,
            quantity,
            unitOfMeasure,
            unitPrice,
            discountRate,
            taxRate);

        _lines.Add(line);

        RecalculateTotals();
        MarkAsUpdated(updatedAtUtc);
    }

    // Product-backed invoice lines
    // -----------------------------------------------------------------

    public void AddProductLine(
        Guid lineId,
        Product product,
        Quantity quantity,
        Percentage discountRate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        EnsureUniqueLineId(lineId);

        if (product.TenantId != TenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!product.IsActive)
            throw new DomainException(DomainErrors.ProductInactive);

        EnsureCurrencyMatches(product.UnitPrice);

        var line = InvoiceLine.CreateFromProduct(
            TenantId,
            lineId,
            Id,
            GetNextPosition(),
            product,
            quantity,
            discountRate);

        _lines.Add(line);

        RecalculateTotals();
        MarkAsUpdated(updatedAtUtc);
    }

    // Line updates
    // -----------------------------------------------------------------

    public void UpdateLine(
        Guid lineId,
        InvoiceLineDescription description,
        Quantity quantity,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage discountRate,
        Percentage taxRate,
        DateTime updatedAtUtc)
    {
        EnsureDraft();
        EnsureCurrencyMatches(unitPrice);

        var line = GetLine(lineId);

        line.Update(
            description,
            quantity,
            unitOfMeasure,
            unitPrice,
            discountRate,
            taxRate);

        RecalculateTotals();
        MarkAsUpdated(updatedAtUtc);
    }

    public void ReplaceLineProduct(
        Guid lineId,
        Product product,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (product.TenantId != TenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!product.IsActive)
            throw new DomainException(DomainErrors.ProductInactive);

        EnsureCurrencyMatches(product.UnitPrice);

        var line = GetLine(lineId);

        line.ReplaceProduct(product);

        RecalculateTotals();
        MarkAsUpdated(updatedAtUtc);
    }

    public void RemoveLine(
        Guid lineId,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        var line = GetLine(lineId);

        _lines.Remove(line);

        ReorderLines();
        RecalculateTotals();
        MarkAsUpdated(updatedAtUtc);
    }

    public void MoveLine(
        Guid lineId,
        int newPosition,
        DateTime updatedAtUtc)
    {
        EnsureDraft();

        if (newPosition <= 0 || newPosition > _lines.Count)
            throw new DomainException(DomainErrors.PositionInvalid);

        var line = GetLine(lineId);
        var currentPosition = line.Position;

        if (currentPosition == newPosition)
            return;

        _lines.Remove(line);
        _lines.Insert(newPosition - 1, line);

        ReorderLines();
        MarkAsUpdated(updatedAtUtc);
    }

    // Lifecycle
    // -----------------------------------------------------------------

    public void Issue(InvoiceNumber number, DateTime issuedAtUtc)
    {
        EnsureDraft();

        if (issuedAtUtc == default)
            throw new DomainException(DomainErrors.IssuedAtUtcRequired);

        if (issuedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.IssuedAtUtcNotUtc);

        if (_lines.Count == 0)
            throw new DomainException(DomainErrors.NoLines);

        if (GrossAmount.Amount <= 0m)
            throw new DomainException(DomainErrors.GrossAmountZero);

        Number = number;
        Status = InvoiceStatus.Issued;
        IssuedAtUtc = issuedAtUtc;

        MarkAsUpdated(issuedAtUtc);
    }

    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status != InvoiceStatus.Issued)
            throw new DomainException(DomainErrors.InvoiceNotIssued);

        if (cancelledAtUtc == default)
            throw new DomainException(DomainErrors.CancelledAtUtcRequired);

        if (cancelledAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CancelledAtUtcNotUtc);

        if (PaidAmount.Amount > 0m)
            throw new DomainException(DomainErrors.InvoicePaidAmountInvalid);

        Status = InvoiceStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;

        MarkAsUpdated(cancelledAtUtc);
    }

    // Payments
    // -----------------------------------------------------------------

    internal void ApplyPayment(Money amount, DateTime updatedAtUtc)
    {
        EnsureCanReceivePayment();
        EnsureCurrencyMatches(amount);

        if (amount.Amount <= 0m)
            throw new DomainException(DomainErrors.PaymentAmountInvalid);

        var outstandingAmount = GrossAmount.Amount - PaidAmount.Amount;

        if (amount.Amount > outstandingAmount)
            throw new DomainException(DomainErrors.PaymentAmountExceedsOutstanding);

        PaidAmount = Money.Create(PaidAmount.Amount + amount.Amount, CurrencyCode);

        MarkAsUpdated(updatedAtUtc);
    }

    internal void ReversePayment(Money amount, DateTime updatedAtUtc)
    {
        EnsureCanReceivePayment();

        if (amount.Amount <= 0m)
            throw new DomainException(DomainErrors.PaymentAmountInvalid);

        if (amount.Amount > PaidAmount.Amount)
            throw new DomainException(DomainErrors.PaymentAmountExceedsPaidAmount);

        PaidAmount = Money.Create(PaidAmount.Amount - amount.Amount, CurrencyCode);

        MarkAsUpdated(updatedAtUtc);
    }

    // Derived domain information
    // -----------------------------------------------------------------

    public bool IsOverdue(DateOnly asOfDate)
    {
        return Status == InvoiceStatus.Issued &&
            DueDate < asOfDate &&
            PaidAmount.Amount < GrossAmount.Amount;
    }

    public bool CanReceivePayment()
    {
        return Status == InvoiceStatus.Issued &&
            PaidAmount.Amount < GrossAmount.Amount;
    }

    // Internal helpers
    // -----------------------------------------------------------------

    [MemberNotNull(
        nameof(CustomerCode),
        nameof(CustomerName),
        nameof(CustomerRegistrationNumber),
        nameof(CustomerAddressLine1),
        nameof(CustomerCity),
        nameof(CustomerCountry),
        nameof(CustomerPostalCode))]
    private void CopyCustomerSnapshot(InvoiceCustomerSnapshot customer)
    {
        CustomerId = customer.CustomerId;

        CustomerCode = customer.CustomerCode;
        CustomerName = customer.CustomerName;
        CustomerEmail = customer.CustomerEmail;
        CustomerPhone = customer.CustomerPhone;
        CustomerTaxNumber = customer.CustomerTaxNumber;
        CustomerRegistrationNumber = customer.CustomerRegistrationNumber;

        CustomerAddressLine1 = customer.CustomerAddressLine1;
        CustomerAddressLine2 = customer.CustomerAddressLine2;
        CustomerCity = customer.CustomerCity;
        CustomerState = customer.CustomerState;
        CustomerCountry = customer.CustomerCountry;
        CustomerPostalCode = customer.CustomerPostalCode;
    }

    private void RecalculateTotals()
    {
        DiscountAmount = Money.Create(_lines.Sum(x => x.DiscountAmount.Amount), CurrencyCode);
        NetAmount = Money.Create(_lines.Sum(x => x.NetAmount.Amount), CurrencyCode);
        TaxAmount = Money.Create(_lines.Sum(x => x.TaxAmount.Amount), CurrencyCode);
        GrossAmount = Money.Create(_lines.Sum(x => x.GrossAmount.Amount), CurrencyCode);
    }

    private InvoiceLine GetLine(Guid lineId)
    {
        if (lineId == Guid.Empty)
            throw new DomainException(DomainErrors.LineIdInvalid);

        return _lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new DomainException(DomainErrors.LineIdNotFound);
    }

    private void EnsureUniqueLineId(Guid lineId)
    {
        if (lineId == Guid.Empty)
            throw new DomainException(DomainErrors.LineIdInvalid);

        if (_lines.Any(x => x.Id == lineId))
            throw new DomainException(DomainErrors.LineIdDuplicate);
    }

    private int GetNextPosition()
    {
        return _lines.Count + 1;
    }

    private void ReorderLines()
    {
        for (var index = 0; index < _lines.Count; index++)
        {
            _lines[index].ChangePosition(index + 1);
        }
    }

    private void EnsureCurrencyMatches(Money money)
    {
        if (money.CurrencyCode != CurrencyCode)
            throw new DomainException(DomainErrors.CurrencyMismatch);
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException(DomainErrors.InvoiceNotDraft);
    }

    private void EnsureCanReceivePayment()
    {
        if (Status != InvoiceStatus.Issued)
            throw new DomainException(DomainErrors.InvoiceNotIssued);
    }

    private void MarkAsUpdated(DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        if (updatedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.UpdatedAtUtcNotUtc);

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateDates(
        DateOnly issueDate,
        DateOnly dueDate)
    {
        if (issueDate == default)
            throw new DomainException(DomainErrors.IssueDateRequired);

        if (dueDate == default)
            throw new DomainException(DomainErrors.DueDateRequired);

        if (dueDate < issueDate)
            throw new DomainException(DomainErrors.DueDateInvalid);
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
}
