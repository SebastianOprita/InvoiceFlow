using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class IssueInvoiceCommandValidator
    : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.InvoiceId)
            .NotEmpty();
    }

}
