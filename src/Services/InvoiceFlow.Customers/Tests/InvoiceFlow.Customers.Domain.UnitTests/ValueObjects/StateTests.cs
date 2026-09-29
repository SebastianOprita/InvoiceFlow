using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class StateTests
{
    [Fact]
    public void Create_ShouldReturnState_WhenValueIsValid()
    {
        var result = State.Create("California");

        result.Value.Should().Be("California");
        result.ToString().Should().Be("California");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = State.Create("  California  ");

        result.Value.Should().Be("California");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => State.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.StateRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => State.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.StateRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', State.MaxLength);

        var result = State.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(State.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', State.MaxLength + 1);

        var act = () => State.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.StateTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', State.MaxLength)}  ";

        var result = State.Create(value);

        result.Value.Should().Be(new string('A', State.MaxLength));
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = State.CreateOptional(null);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void CreateOptional_ShouldReturnNull_WhenValueIsWhiteSpace(string value)
    {
        var result = State.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnState_WhenValueIsValid()
    {
        var result = State.CreateOptional("California");

        result.Should().NotBeNull();
        result!.Value.Should().Be("California");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = State.CreateOptional("  California  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("California");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', State.MaxLength + 1);

        var act = () => State.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.StateTooLong.ErrorMessage);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = State.Create("  California  ");

        result.ToString().Should().Be("California");
    }
}
