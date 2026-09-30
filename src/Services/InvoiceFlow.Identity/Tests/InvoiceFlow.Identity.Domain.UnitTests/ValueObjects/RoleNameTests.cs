using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class RoleNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateRoleName()
    {
        var roleName = RoleName.Create("Administrator");

        roleName.Value.Should().Be("Administrator");
        roleName.ToString().Should().Be("Administrator");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var roleName = RoleName.Create("  Manager  ");

        roleName.Value.Should().Be("Manager");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => RoleName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.NameRequired.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', RoleName.MaxLength + 1);

        var act = () => RoleName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.NameTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateRoleName()
    {
        var value = new string('a', RoleName.MaxLength);

        var roleName = RoleName.Create(value);

        roleName.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var roleName = RoleName.Create("User");

        roleName.ToString().Should().Be("User");
    }
}
