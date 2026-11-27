using FluentValidation;

namespace InvoiceFlow.Payments.Application;

internal sealed class AddPaymentAllocationCommandValidator : AbstractValidator<AddPaymentAllocationCommand>
{
    public AddPaymentAllocationCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().
            MaximumLength(128);

        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.PaymentId)
            .NotEmpty();

        RuleFor(x => x.InvoiceId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0m);

        RuleFor(x => x.AllocatedBy)
            .NotEmpty();
    }
}
