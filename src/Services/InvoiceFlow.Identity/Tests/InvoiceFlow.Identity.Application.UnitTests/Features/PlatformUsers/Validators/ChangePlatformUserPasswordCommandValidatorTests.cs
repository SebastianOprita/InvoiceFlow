using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class ChangePlatformUserPasswordCommandValidatorTests
{
    private readonly ChangePlatformUserPasswordCommandValidator _validator;

    public ChangePlatformUserPasswordCommandValidatorTests()
    {
        _validator = new ChangePlatformUserPasswordCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ChangePlatformUserPasswordCommand(
            Guid.Empty,
            "OldPassword123",
            "NewPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_CurrentPassword_Is_Empty()
    {
        // Arrange
        var command = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            string.Empty,
            "NewPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Empty()
    {
        // Arrange
        var command = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "OldPassword123",
            string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "OldPassword123",
            "NewPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }
}