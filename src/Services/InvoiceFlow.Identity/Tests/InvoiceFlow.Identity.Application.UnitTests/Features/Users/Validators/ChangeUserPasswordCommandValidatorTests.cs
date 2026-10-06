using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class ChangeUserPasswordCommandValidatorTests
{
    private readonly ChangeUserPasswordCommandValidator _validator;

    public ChangeUserPasswordCommandValidatorTests()
    {
        _validator = new ChangeUserPasswordCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new ChangeUserPasswordCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            "OldPassword123",
            "NewPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
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
        var command = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
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
        var command = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
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
        var command = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "OldPassword123",
            "NewPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }
}