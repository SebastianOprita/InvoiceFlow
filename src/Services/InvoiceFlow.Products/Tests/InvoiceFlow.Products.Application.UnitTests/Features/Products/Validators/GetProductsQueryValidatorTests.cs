using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Products.Application.UnitTests.Features.Validators;

public sealed class GetProductsQueryValidatorTests
{
    private readonly GetProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenTenantIdIsNotEmpty()
    {
        var query = new GetProductsQuery(TenantId: Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTenantIdIsEmpty()
    {
        var query = new GetProductsQuery(TenantId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }
}
