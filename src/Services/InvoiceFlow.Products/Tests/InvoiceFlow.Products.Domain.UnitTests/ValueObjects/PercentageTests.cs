using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class PercentageTests
{
    [Fact]
    public void Create_ShouldReturnPercentage_WhenValueIsValid()
    {
        var result = Percentage.Create(25m);

        result.Value.Should().Be(25m);
    }

    [Fact]
    public void Create_ShouldAllowZero()
    {
        var result = Percentage.Create(0m);

        result.Value.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldAllowOneHundred()
    {
        var result = Percentage.Create(100m);

        result.Value.Should().Be(100m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(101)]
    public void Create_ShouldThrowDomainException_WhenValueIsOutsideValidRange(
        decimal value)
    {
        var act = () => Percentage.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PercentageInvalid);
    }

    [Fact]
    public void Zero_ShouldReturnPercentageWithZeroValue()
    {
        Percentage.Zero.Value.Should().Be(0m);
    }

    [Fact]
    public void ToString_ShouldAppendPercentSign()
    {
        var result = Percentage.Create(25m);

        result.ToString().Should().Be("25%");
    }
}
