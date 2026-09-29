using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public sealed class GetTenantsQueryValidatorTests
{
    private readonly GetTenantsQueryValidator _validator;

    public GetTenantsQueryValidatorTests()
    {
        _validator = new GetTenantsQueryValidator();
    }

    [Fact]
    public void Should_Not_Have_Errors()
    {
        // Arrange
        var query = new GetTenantsQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
