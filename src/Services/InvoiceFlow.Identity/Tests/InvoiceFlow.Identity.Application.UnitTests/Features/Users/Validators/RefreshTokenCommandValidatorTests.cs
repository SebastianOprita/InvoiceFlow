using FluentValidation.TestHelper;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = new RefreshTokenCommand(
            Guid.Empty,
            "refresh-token",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        var command = new RefreshTokenCommand(
            Guid.CreateVersion7(),
            string.Empty,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Should_Have_Error_When_DeviceInfo_Is_Too_Long()
    {
        var command = new RefreshTokenCommand(
            Guid.CreateVersion7(),
            "refresh-token",
            new string('A', DeviceInfo.MaxLength + 1),
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeviceInfo);
    }

    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Too_Long()
    {
        var command = new RefreshTokenCommand(
            Guid.CreateVersion7(),
            "refresh-token",
            null,
            new string('A', IpAddress.MaxLength + 1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Should_Not_Have_Error_When_DeviceInfo_And_IpAddress_Are_Null()
    {
        var command = new RefreshTokenCommand(
            Guid.CreateVersion7(),
            "refresh-token",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new RefreshTokenCommand(
            Guid.CreateVersion7(),
            "refresh-token",
            "Chrome on Windows",
            "127.0.0.1");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}