using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CreditLimitTests
{
    [Fact]
    public void Create_ShouldReturnCreditLimit_WhenValueIsPositive()
    {
        var result = CreditLimit.Create(1500.50m);

        result.Value.Should().Be(1500.50m);
        result.ToString().Should().Be("1500.50");
    }

    [Fact]
    public void Create_ShouldReturnCreditLimit_WhenValueIsZero()
    {
        var result = CreditLimit.Create(0m);

        result.Value.Should().Be(0m);
        result.ToString().Should().Be("0");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNegative()
    {
        var act = () => CreditLimit.Create(-1m);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CreditLimitInvalid);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-999999.99)]
    public void Create_ShouldThrowDomainException_ForAnyNegativeValue(decimal value)
    {
        var act = () => CreditLimit.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CreditLimitInvalid);
    }

    [Fact]
    public void ToString_ShouldReturnDecimalStringRepresentation()
    {
        var result = CreditLimit.Create(2500.75m);

        result.ToString().Should().Be("2500.75");
    }
}
