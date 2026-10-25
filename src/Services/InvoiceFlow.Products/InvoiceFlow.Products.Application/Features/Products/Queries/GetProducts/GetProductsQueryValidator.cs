using FluentValidation;

namespace InvoiceFlow.Products.Application;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
