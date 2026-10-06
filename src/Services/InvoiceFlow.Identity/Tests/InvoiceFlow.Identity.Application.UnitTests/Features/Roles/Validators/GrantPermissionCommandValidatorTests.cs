using FluentValidation.TestHelper;
using InvoiceFlow.BuildingBlocks.Authorization;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class GrantPermissionCommandValidatorTests
{
    private readonly GrantPermissionCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new GrantPermissionCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RoleId_Is_Empty()
    {
        // Arrange
        var command = new GrantPermissionCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Should_Have_Error_When_Permission_Is_Invalid()
    {
        // Arrange
        var invalidPermission = (SystemPermission)999999;

        var command = new GrantPermissionCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            invalidPermission);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Permission)
            .WithErrorMessage("Permission contain invalid flags.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new GrantPermissionCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
