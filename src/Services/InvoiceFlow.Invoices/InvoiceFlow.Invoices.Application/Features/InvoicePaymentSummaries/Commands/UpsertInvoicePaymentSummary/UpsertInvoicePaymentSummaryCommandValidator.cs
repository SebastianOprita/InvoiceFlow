using FluentValidation;

namespace InvoiceFlow.Invoices.Application;

internal sealed class UpsertInvoicePaymentSummaryCommandValidator
    : AbstractValidator<UpsertInvoicePaymentSummaryCommand>
{
    public UpsertInvoicePaymentSummaryCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.InvoiceId)
            .NotEmpty();

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0m);

        RuleFor(x => x.OutstandingAmount)
            .GreaterThanOrEqualTo(0m);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.PaymentBalanceVersion)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SourceUpdatedAtUtc)
            .NotEqual(default(DateTimeOffset));

        RuleFor(x => x.AppliedAtUtc)
            .NotEqual(default(DateTimeOffset));
    }
}
