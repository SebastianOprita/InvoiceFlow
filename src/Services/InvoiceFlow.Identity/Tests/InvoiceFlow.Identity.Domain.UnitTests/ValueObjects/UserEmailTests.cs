using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class UserEmailTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateUserEmail()
    {
        var email = UserEmail.Create("john.doe@example.com");

        email.Value.Should().Be("john.doe@example.com");
        email.ToString().Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_ShouldTrimAndNormalizeToLowercase()
    {
        var email = UserEmail.Create("  John.Doe@Example.COM  ");

        email.Value.Should().Be("john.doe@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => UserEmail.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.EmailRequired.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', UserEmail.MaxLength - "@a.com".Length + 1) + "@a.com";

        var act = () => UserEmail.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.EmailTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateUserEmail()
    {
        var localPartLength = UserEmail.MaxLength - "@a.com".Length;
        var value = new string('a', localPartLength) + "@a.com";

        var email = UserEmail.Create(value);

        email.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("@example.com")]
    [InlineData("john.doe@")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string value)
    {
        var act = () => UserEmail.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.EmailInvalid.ErrorMessage);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.ro")]
    [InlineData("admin123@company.org")]
    public void Create_WithValidEmail_ShouldCreateUserEmail(string value)
    {
        var email = UserEmail.Create(value);

        email.Value.Should().Be(value.ToLowerInvariant());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var email = UserEmail.Create("user@example.com");

        email.ToString().Should().Be("user@example.com");
    }
}
