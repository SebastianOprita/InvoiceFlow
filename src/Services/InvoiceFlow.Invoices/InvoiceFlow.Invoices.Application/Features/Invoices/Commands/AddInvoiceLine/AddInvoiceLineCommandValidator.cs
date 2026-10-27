using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class AddInvoiceLineCommandValidator : AbstractValidator<AddInvoiceLineCommand>
{
    public AddInvoiceLineCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty();

        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.InvoiceId)
            .NotEmpty();

        RuleFor(x => x.ProductCode)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.DiscountRate)
            .InclusiveBetween(0m, 100m)
            .When(x => x.DiscountRate.HasValue);
    }
}
