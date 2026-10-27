using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

public sealed class InvoiceNumberSequenceTests
{
    [Fact]
    public void Create_SetsSequenceProperties()
    {
        var tenantId = Guid.CreateVersion7();
        var id = Guid.CreateVersion7();
        var year = InvoiceYear.Create(2025);
        var prefix = InvoiceNumberPrefix.Create("INV");
        var nextNumber = SequenceNumber.Create(7);

        var sequence = InvoiceNumberSequence.Create(tenantId, id, year, prefix, nextNumber);

        sequence.TenantId.Should().Be(tenantId);
        sequence.Id.Should().Be(id);
        sequence.Year.Should().Be(year);
        sequence.Prefix.Should().Be(prefix);
        sequence.NextNumber.Should().Be(nextNumber);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        Action act = () => InvoiceNumberSequence.Create(Guid.Empty, Guid.CreateVersion7(), InvoiceYear.Create(2025),
            InvoiceNumberPrefix.Create("INV"), SequenceNumber.Create(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyId()
    {
        Action act = () => InvoiceNumberSequence.Create(Guid.CreateVersion7(), Guid.Empty, InvoiceYear.Create(2025),
            InvoiceNumberPrefix.Create("INV"), SequenceNumber.Create(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void AllocateNextNumber_FormatsNumberAndAdvancesSequence()
    {
        var sequence = InvoiceNumberSequence.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceYear.Create(2025),
            InvoiceNumberPrefix.Create("INV"), SequenceNumber.Create(7));

        sequence.AllocateNextNumber().Should().Be("INV-2025-000007");
        sequence.NextNumber.Value.Should().Be(8);
        sequence.AllocateNextNumber().Should().Be("INV-2025-000008");
        sequence.NextNumber.Value.Should().Be(9);
    }
}
