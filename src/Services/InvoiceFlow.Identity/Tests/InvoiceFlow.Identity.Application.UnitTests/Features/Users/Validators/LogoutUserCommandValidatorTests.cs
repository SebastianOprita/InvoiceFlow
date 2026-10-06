using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class LogoutUserCommandValidatorTests
{
    private readonly LogoutUserCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new LogoutUserCommand(
            Guid.Empty,
            "refresh-token");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        // Arrange
        var command = new LogoutUserCommand(
            Guid.CreateVersion7(),
            string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new LogoutUserCommand(
            Guid.CreateVersion7(),
            "refresh-token");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}