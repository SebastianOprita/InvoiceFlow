using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class RefreshTokenHashTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateRefreshTokenHash()
    {
        var tokenHash = RefreshTokenHash.Create("refresh-token-hash");

        tokenHash.Value.Should().Be("refresh-token-hash");
        tokenHash.ToString().Should().Be("refresh-token-hash");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var tokenHash = RefreshTokenHash.Create("  refresh-token-hash  ");

        tokenHash.Value.Should().Be("refresh-token-hash");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => RefreshTokenHash.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.TokenHashRequired.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateRefreshTokenHash()
    {
        var value = new string('a', RefreshTokenHash.MaxLength);

        var tokenHash = RefreshTokenHash.Create(value);

        tokenHash.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var tokenHash = RefreshTokenHash.Create("token-hash");

        tokenHash.ToString().Should().Be("token-hash");
    }
}
