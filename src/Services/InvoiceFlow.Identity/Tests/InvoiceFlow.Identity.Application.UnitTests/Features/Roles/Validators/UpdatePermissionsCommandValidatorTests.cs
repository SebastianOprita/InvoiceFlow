using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class UpdatePermissionsCommandValidatorTests
{
    private readonly UpdatePermissionsCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new UpdatePermissionsCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            1L << 3);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RoleId_Is_Empty()
    {
        // Arrange
        var command = new UpdatePermissionsCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            1L << 3);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Should_Have_Error_When_Permissions_Are_Invalid()
    {
        // Arrange
        var invalidPermissions = (long)999999;

        var command = new UpdatePermissionsCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            invalidPermissions);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage("Permissions contain invalid flags.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdatePermissionsCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1L << 3 | 1L << 4);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
