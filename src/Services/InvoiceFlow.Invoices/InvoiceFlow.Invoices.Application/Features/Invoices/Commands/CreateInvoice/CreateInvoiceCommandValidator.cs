using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.CustomerCode)
            .NotEmpty()
            .MaximumLength(38);

        RuleFor(x => x.Notes)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
