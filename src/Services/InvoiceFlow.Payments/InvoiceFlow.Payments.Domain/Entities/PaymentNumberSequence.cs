using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed class PaymentNumberSequence
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public FiscalYear Year { get; private set; }
    public PaymentNumberPrefix Prefix { get; private set; }
    public SequenceNumber NextNumber { get; private set; }

#pragma warning disable CS8618
    private PaymentNumberSequence() { }
#pragma warning restore CS8618

    private PaymentNumberSequence(
        Guid tenantId,
        Guid id,
        FiscalYear year,
        PaymentNumberPrefix prefix,
        SequenceNumber startingNumber)
    {
        TenantId = tenantId;
        Id = id;
        Year = year;
        Prefix = prefix;
        NextNumber = startingNumber;
    }

    public static PaymentNumberSequence Create(
        Guid tenantId,
        Guid id,
        FiscalYear year,
        PaymentNumberPrefix prefix,
        SequenceNumber startingNumber)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        return new PaymentNumberSequence(
            tenantId,
            id,
            year,
            prefix,
            startingNumber);
    }

    public string AllocateNextNumber()
    {
        var invoiceNumber = $"{Prefix.Value}-{Year.Value}-{NextNumber.Value:D6}";
        NextNumber = NextNumber.Next();

        return invoiceNumber;
    }
}
