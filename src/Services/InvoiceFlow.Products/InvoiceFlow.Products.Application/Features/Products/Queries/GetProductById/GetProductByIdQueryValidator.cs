using FluentValidation;

namespace InvoiceFlow.Products.Application.Features;

internal sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.ProductId).NotEmpty();
    }
}
