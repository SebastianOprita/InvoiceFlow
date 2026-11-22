using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.ValueObjects;

public sealed class ReversalReasonTests
{
    [Fact]
    public void Create_trims_reason()
    {
        ReversalReason.Create("  Duplicate receipt  ").Value.Should().Be("Duplicate receipt");
    }

    [Fact]
    public void Create_rejects_blank_reason()
    {
        Action act = () => ReversalReason.Create("  ");
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.ReversalReasonRequired);
    }

    [Fact]
    public void Create_rejects_reason_over_maximum_length()
    {
        Action act = () => ReversalReason.Create(new string('x', ReversalReason.MaxLength + 1));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.ReversalReasonTooLong);
    }
}
