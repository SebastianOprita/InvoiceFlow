using FluentValidation;

namespace InvoiceFlow.Payments.Application;

internal sealed class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.PaymentId).NotEmpty();
    }
}
