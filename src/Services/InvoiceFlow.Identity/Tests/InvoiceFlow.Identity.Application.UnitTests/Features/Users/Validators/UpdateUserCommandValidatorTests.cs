using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator;

    public UpdateUserCommandValidatorTests()
    {
        _validator = new UpdateUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            string.Empty,
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "John",
            string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
}