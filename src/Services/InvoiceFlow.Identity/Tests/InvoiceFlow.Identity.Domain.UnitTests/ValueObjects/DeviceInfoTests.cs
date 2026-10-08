using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class DeviceInfoTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateDeviceInfo()
    {
        var deviceInfo = DeviceInfo.Create("Chrome on Windows");

        deviceInfo.Value.Should().Be("Chrome on Windows");
        deviceInfo.ToString().Should().Be("Chrome on Windows");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var deviceInfo = DeviceInfo.Create("  Safari on iPhone  ");

        deviceInfo.Value.Should().Be("Safari on iPhone");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => DeviceInfo.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.DeviceInfoRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', DeviceInfo.MaxLength + 1);

        var act = () => DeviceInfo.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.DeviceInfoTooLong);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateDeviceInfo()
    {
        var value = new string('a', DeviceInfo.MaxLength);

        var deviceInfo = DeviceInfo.Create(value);

        deviceInfo.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateOptional_WithNullOrWhiteSpace_ShouldReturnNull(string? value)
    {
        var deviceInfo = DeviceInfo.CreateOptional(value);

        deviceInfo.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreateDeviceInfo()
    {
        var deviceInfo = DeviceInfo.CreateOptional("  Firefox on Linux  ");

        deviceInfo.Should().NotBeNull();
        deviceInfo!.Value.Should().Be("Firefox on Linux");
    }
}
