using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class LastNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateLastName()
    {
        var lastName = LastName.Create("Doe");

        lastName.Value.Should().Be("Doe");
        lastName.ToString().Should().Be("Doe");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var lastName = LastName.Create("  Smith  ");

        lastName.Value.Should().Be("Smith");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => LastName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.LastNameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', LastName.MaxLength + 1);

        var act = () => LastName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.LastNameTooLong);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateLastName()
    {
        var value = new string('a', LastName.MaxLength);

        var lastName = LastName.Create(value);

        lastName.Value.Should().Be(value);
    }
}
