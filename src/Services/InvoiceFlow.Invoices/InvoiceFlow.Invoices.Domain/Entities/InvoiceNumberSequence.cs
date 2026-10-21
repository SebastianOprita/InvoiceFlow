using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed class InvoiceNumberSequence
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public InvoiceYear Year { get; private set; }
    public InvoiceNumberPrefix Prefix { get; private set; }
    public SequenceNumber NextNumber { get; private set; }

#pragma warning disable CS8618
    private InvoiceNumberSequence() { }
#pragma warning restore CS8618

    public InvoiceNumberSequence(
        Guid tenantId,
        Guid id,
        InvoiceYear year,
        InvoiceNumberPrefix prefix,
        SequenceNumber startingNumber)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        TenantId = tenantId;
        Id = id;
        Year = year;
        Prefix = prefix;
        NextNumber = startingNumber;
    }

    public string AllocateNextNumber()
    {
        var invoiceNumber = $"{Prefix.Value}-{Year.Value}-{NextNumber.Value:D6}";
        NextNumber = NextNumber.Next();

        return invoiceNumber;
    }
}
