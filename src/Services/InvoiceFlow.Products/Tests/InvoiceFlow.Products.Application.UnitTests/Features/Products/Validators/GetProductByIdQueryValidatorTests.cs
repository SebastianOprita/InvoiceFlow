using FluentValidation.TestHelper;
using InvoiceFlow.Products.Application.Features;
using Xunit;

namespace InvoiceFlow.Products.Application.UnitTests.Features.Validators;

public sealed class GetProductByIdQueryValidatorTests
{
    private readonly GetProductByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenQueryIsValid()
    {
        var query = new GetProductByIdQuery(
            TenantId: Guid.NewGuid(),
            ProductId: Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTenantIdIsEmpty()
    {
        var query = new GetProductByIdQuery(
            TenantId: Guid.Empty,
            ProductId: Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenProductIdIsEmpty()
    {
        var query = new GetProductByIdQuery(
            TenantId: Guid.NewGuid(),
            ProductId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
}
