using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class DeleteRoleCommandValidatorTests
{
    private readonly DeleteRoleCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new DeleteRoleCommand(
            Guid.Empty,
            Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RoleId_Is_Empty()
    {
        // Arrange
        var command = new DeleteRoleCommand(
            Guid.CreateVersion7(),
            Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new DeleteRoleCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
