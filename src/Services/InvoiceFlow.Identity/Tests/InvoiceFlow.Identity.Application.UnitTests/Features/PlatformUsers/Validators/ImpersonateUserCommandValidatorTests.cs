using FluentValidation.TestHelper;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Platform.PlatformUsers;

public class ImpersonateUserCommandValidatorTests
{
    private readonly ImpersonateUserCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ActorUserId_Is_Empty()
    {
        var command = ValidCommand() with { ActorUserId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActorUserId);
    }

    [Fact]
    public void Should_Have_Error_When_TargetUserTenantId_Is_Empty()
    {
        var command = ValidCommand() with { TargetUserTenantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserTenantId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_TargetUserEmail_Is_Empty(string? email)
    {
        var command = ValidCommand() with { TargetUserEmail = email! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserEmail);
    }

    [Fact]
    public void Should_Have_Error_When_DeviceInfo_Exceeds_MaxLength()
    {
        var command = ValidCommand() with
        {
            DeviceInfo = new string('a', DeviceInfo.MaxLength + 1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeviceInfo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Not_Have_Error_When_DeviceInfo_Is_Null_Or_Whitespace(string? deviceInfo)
    {
        var command = ValidCommand() with { DeviceInfo = deviceInfo };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DeviceInfo);
    }

    [Fact]
    public void Should_Have_Error_When_IpAddress_Exceeds_MaxLength()
    {
        var command = ValidCommand() with
        {
            IpAddress = new string('1', IpAddress.MaxLength + 1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Not_Have_Error_When_IpAddress_Is_Null_Or_Whitespace(string? ipAddress)
    {
        var command = ValidCommand() with { IpAddress = ipAddress };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Should_Not_Have_Any_Errors_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_For_Any_Reason_Value()
    {
        var command = ValidCommand() with
        {
            Reason = new string('a', 10_000)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    private static ImpersonateUserCommand ValidCommand() =>
        new(
            ActorUserId: Guid.CreateVersion7(),
            TargetUserTenantId: Guid.CreateVersion7(),
            TargetUserEmail: "target@example.com",
            Reason: "Support request",
            DeviceInfo: new string('a', DeviceInfo.MaxLength),
            IpAddress: new string('1', IpAddress.MaxLength));
}