using FluentValidation.TestHelper;
using InvoiceFlow.Identity.Application.Features;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class CreatePlatformUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreatePlatformUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.Empty,
            "john.doe@example.com",
            "Password123",
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.Empty,
            string.Empty,
            "Password123",
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "invalid-email",
            "Password123",
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@example.com",
            string.Empty,
            "John",
            "Doe");


        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@example.com",
            "1234567",
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@example.com",
            "Password123",
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
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@example.com",
            "Password123",
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
        var command = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@example.com",
            "Password123",
            "John",
            "Doe");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
}