using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class RoleDescriptionTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateRoleDescription()
    {
        var description = RoleDescription.Create("Administrator role");

        description.Value.Should().Be("Administrator role");
        description.ToString().Should().Be("Administrator role");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var description = RoleDescription.Create("  Finance manager role  ");

        description.Value.Should().Be("Finance manager role");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => RoleDescription.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.DescriptionRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', RoleDescription.MaxLength + 1);

        var act = () => RoleDescription.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.DescriptionTooLong);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateRoleDescription()
    {
        var value = new string('a', RoleDescription.MaxLength);

        var description = RoleDescription.Create(value);

        description.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateOptional_WithNullOrWhiteSpace_ShouldReturnNull(string? value)
    {
        var description = RoleDescription.CreateOptional(value);

        description.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreateRoleDescription()
    {
        var description = RoleDescription.CreateOptional("  Standard user role  ");

        description.Should().NotBeNull();
        description!.Value.Should().Be("Standard user role");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var description = RoleDescription.Create("Manager role");

        description.ToString().Should().Be("Manager role");
    }
}
