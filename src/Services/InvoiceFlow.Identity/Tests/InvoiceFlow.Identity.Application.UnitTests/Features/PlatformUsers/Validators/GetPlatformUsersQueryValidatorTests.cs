using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public sealed class GetPlatformUsersQueryValidatorTests
{
    private readonly GetPlatformUsersQueryValidator _validator;

    public GetPlatformUsersQueryValidatorTests()
    {
        _validator = new GetPlatformUsersQueryValidator();
    }

    [Fact]
    public void Should_Not_Have_Errors()
    {
        // Arrange
        var query = new GetPlatformUsersQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
