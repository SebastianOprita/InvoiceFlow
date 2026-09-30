using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class IpAddressTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateIpAddress()
    {
        var ipAddress = IpAddress.Create("192.168.1.1");

        ipAddress.Value.Should().Be("192.168.1.1");
        ipAddress.ToString().Should().Be("192.168.1.1");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var ipAddress = IpAddress.Create("  10.0.0.1  ");

        ipAddress.Value.Should().Be("10.0.0.1");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => IpAddress.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.IpAddressRequired.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', IpAddress.MaxLength + 1);

        var act = () => IpAddress.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.IpAddressTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateIpAddress()
    {
        var value = new string('a', IpAddress.MaxLength);

        var ipAddress = IpAddress.Create(value);

        ipAddress.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateOptional_WithNullOrWhiteSpace_ShouldReturnNull(string? value)
    {
        var ipAddress = IpAddress.CreateOptional(value);

        ipAddress.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreateIpAddress()
    {
        var ipAddress = IpAddress.CreateOptional("  127.0.0.1  ");

        ipAddress.Should().NotBeNull();
        ipAddress!.Value.Should().Be("127.0.0.1");
    }
}
