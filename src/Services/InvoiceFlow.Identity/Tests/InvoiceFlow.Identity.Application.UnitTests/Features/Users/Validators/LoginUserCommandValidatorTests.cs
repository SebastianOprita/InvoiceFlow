using FluentValidation.TestHelper;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = new LoginUserCommand(
            Guid.Empty,
            "user@test.com",
            "Password123!",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            string.Empty,
            "Password123!",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "invalid-email",
            "Password123!",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            string.Empty,
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_DeviceInfo_Is_Too_Long()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            "Password123!",
            new string('A', DeviceInfo.MaxLength + 1),
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeviceInfo);
    }

    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Too_Long()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            "Password123!",
            null,
            new string('A', IpAddress.MaxLength + 1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Should_Not_Have_Error_When_DeviceInfo_And_IpAddress_Are_Null()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            "Password123!",
            null,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new LoginUserCommand(
            Guid.CreateVersion7(),
            "user@test.com",
            "Password123!",
            "Chrome on Windows",
            "127.0.0.1");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
