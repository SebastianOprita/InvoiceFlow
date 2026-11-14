using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class PaymentNumberSequenceTests
{
    [Fact]
    public void Create_SetsSequenceProperties()
    {
        var tenantId = Guid.CreateVersion7();
        var id = Guid.CreateVersion7();
        var year = FiscalYear.Create(2025);
        var prefix = PaymentNumberPrefix.Create("PAY");
        var nextNumber = SequenceNumber.Create(7);

        var sequence = PaymentNumberSequence.Create(tenantId, id, year, prefix, nextNumber);

        sequence.TenantId.Should().Be(tenantId);
        sequence.Id.Should().Be(id);
        sequence.Year.Should().Be(year);
        sequence.Prefix.Should().Be(prefix);
        sequence.NextNumber.Should().Be(nextNumber);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        Action act = () => PaymentNumberSequence.Create(Guid.Empty, Guid.CreateVersion7(), FiscalYear.Create(2025),
            PaymentNumberPrefix.Create("PAY"), SequenceNumber.Create(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyId()
    {
        Action act = () => PaymentNumberSequence.Create(Guid.CreateVersion7(), Guid.Empty, FiscalYear.Create(2025),
            PaymentNumberPrefix.Create("PAY"), SequenceNumber.Create(1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void AllocateNextNumber_FormatsNumberAndAdvancesSequence()
    {
        var sequence = PaymentNumberSequence.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), FiscalYear.Create(2025),
            PaymentNumberPrefix.Create("PAY"), SequenceNumber.Create(7));

        sequence.AllocateNextNumber().Should().Be("PAY-2025-000007");
        sequence.NextNumber.Value.Should().Be(8);
        sequence.AllocateNextNumber().Should().Be("PAY-2025-000008");
        sequence.NextNumber.Value.Should().Be(9);
    }
}
