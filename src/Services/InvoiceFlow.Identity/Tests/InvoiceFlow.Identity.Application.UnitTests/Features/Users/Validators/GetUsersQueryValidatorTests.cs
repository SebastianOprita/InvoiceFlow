using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class GetUsersQueryValidatorTests
{
    private readonly GetUsersQueryValidator _validator;

    public GetUsersQueryValidatorTests()
    {
        _validator = new GetUsersQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        // Arrange
        var query = new GetUsersQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TenantId_Is_Valid()
    {
        // Arrange
        var query = new GetUsersQuery(Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
    }
}
