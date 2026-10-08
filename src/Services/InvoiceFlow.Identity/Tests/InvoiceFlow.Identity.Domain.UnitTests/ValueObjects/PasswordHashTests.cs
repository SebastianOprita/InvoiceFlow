using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class PasswordHashTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreatePasswordHash()
    {
        var passwordHash = PasswordHash.Create("hashed-password-value");

        passwordHash.Value.Should().Be("hashed-password-value");
        passwordHash.ToString().Should().Be("hashed-password-value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => PasswordHash.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PasswordHashRequired);
    }

    [Fact]
    public void Create_ShouldPreserveOriginalValue()
    {
        var value = "  hashed-password-value  ";

        var passwordHash = PasswordHash.Create(value);

        passwordHash.Value.Should().Be(value);
    }
}
