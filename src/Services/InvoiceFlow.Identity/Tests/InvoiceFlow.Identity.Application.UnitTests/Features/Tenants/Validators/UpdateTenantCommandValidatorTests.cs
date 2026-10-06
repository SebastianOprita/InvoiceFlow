using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformTenants;

public class UpdateTenantCommandValidatorTests
{
    private readonly UpdateTenantCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new UpdateTenantCommand
        (
            Guid.Empty,
            "Updated Tenant"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new UpdateTenantCommand
        (
            Guid.CreateVersion7(),
            string.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateTenantCommand
        (
            Guid.CreateVersion7(),
            "Updated Tenant"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
