using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformTenants;

public class DeactivateTenantCommandValidatorTests
{
    private readonly DeactivateTenantCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var command = new DeactivateTenantCommand
        (
            Guid.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TenantId_Is_Valid()
    {
        // Arrange
        var command = new DeactivateTenantCommand
        (
            Guid.CreateVersion7()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
    }
}
