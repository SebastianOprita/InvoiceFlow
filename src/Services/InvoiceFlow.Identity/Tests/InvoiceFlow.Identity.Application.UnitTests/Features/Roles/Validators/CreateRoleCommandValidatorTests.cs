using FluentValidation.TestHelper;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new CreateRoleCommand(
            Guid.Empty,
            "Admin",
            null,
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new CreateRoleCommand(
            Guid.CreateVersion7(),
            string.Empty,
            null,
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        // Arrange
        var description = new string('A', RoleDescription.MaxLength + 1);

        var command = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            description,
            SystemPermission.CustomerView);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Permissions_Are_Invalid()
    {
        // Arrange
        var invalidPermissions = (SystemPermission)999999;

        var command = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            null,
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
        var command = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            "Administrator role",
            SystemPermission.CustomerView | SystemPermission.CustomerCreate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
