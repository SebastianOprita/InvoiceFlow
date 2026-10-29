using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.InvoiceId).NotEmpty();
    }
}
