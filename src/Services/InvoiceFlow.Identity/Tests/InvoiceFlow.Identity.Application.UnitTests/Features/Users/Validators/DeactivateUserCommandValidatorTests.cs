using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class DeactivatePlatformUserCommandValidatorTests
{
    private readonly DeactivateUserCommandValidator _validator;

    public DeactivatePlatformUserCommandValidatorTests()
    {
        _validator = new DeactivateUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.Empty, Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}