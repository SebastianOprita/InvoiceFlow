using FluentValidation;

namespace InvoiceFlow.Payments.Application;

internal sealed class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
