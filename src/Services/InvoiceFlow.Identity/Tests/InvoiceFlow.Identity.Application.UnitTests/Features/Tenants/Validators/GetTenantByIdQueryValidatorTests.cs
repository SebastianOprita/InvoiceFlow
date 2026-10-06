using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public sealed class GetTenantByIdQueryValidatorTests
{
    private readonly GetTenantByIdQueryValidator _validator;

    public GetTenantByIdQueryValidatorTests()
    {
        _validator = new GetTenantByIdQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var query = new GetTenantByIdQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        // Arrange
        var query = new GetTenantByIdQuery(Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
