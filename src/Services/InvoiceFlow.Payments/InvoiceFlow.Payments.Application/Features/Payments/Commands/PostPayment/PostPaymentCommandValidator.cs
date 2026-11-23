using FluentValidation;

namespace InvoiceFlow.Payments.Application;

internal sealed class PostPaymentCommandValidator : AbstractValidator<PostPaymentCommand>
{
    public PostPaymentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.PostedBy).NotEmpty();
    }
}
