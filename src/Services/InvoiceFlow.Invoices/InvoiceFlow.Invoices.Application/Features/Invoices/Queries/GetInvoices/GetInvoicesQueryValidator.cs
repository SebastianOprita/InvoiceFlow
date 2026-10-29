using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class GetInvoicesQueryValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
