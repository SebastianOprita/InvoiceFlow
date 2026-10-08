using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class FirstNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateFirstName()
    {
        var firstName = FirstName.Create("John");

        firstName.Value.Should().Be("John");
        firstName.ToString().Should().Be("John");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var firstName = FirstName.Create("  Alice  ");

        firstName.Value.Should().Be("Alice");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => FirstName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.FirstNameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', FirstName.MaxLength + 1);

        var act = () => FirstName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.FirstNameTooLong);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateFirstName()
    {
        var value = new string('a', FirstName.MaxLength);

        var firstName = FirstName.Create(value);

        firstName.Value.Should().Be(value);
    }
}
