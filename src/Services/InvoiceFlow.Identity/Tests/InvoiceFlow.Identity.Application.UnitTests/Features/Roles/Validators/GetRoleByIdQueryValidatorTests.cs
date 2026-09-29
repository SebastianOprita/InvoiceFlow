using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public sealed class GetRoleByIdQueryValidatorTests
{
    private readonly GetRoleByIdQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var query = new GetRoleByIdQuery(
            Guid.Empty,
            Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_RoleId_Is_Empty()
    {
        // Arrange
        var query = new GetRoleByIdQuery(
            Guid.CreateVersion7(),
            Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Query_Is_Valid()
    {
        // Arrange
        var query = new GetRoleByIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
