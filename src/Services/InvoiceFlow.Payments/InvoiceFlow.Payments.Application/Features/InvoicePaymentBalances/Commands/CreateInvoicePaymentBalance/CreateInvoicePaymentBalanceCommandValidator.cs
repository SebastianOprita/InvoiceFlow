using FluentValidation;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

internal sealed class CreateInvoicePaymentBalanceCommandValidator
    : AbstractValidator<CreateInvoicePaymentBalanceCommand>
{
    public CreateInvoicePaymentBalanceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(InvoiceNumber.MaxLength);
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeSupportedCurrency)
            .WithMessage("CurrencyCode must be a supported ISO 4217 currency code.");
        RuleFor(x => x.TotalAmount)
            .GreaterThan(0m)
            .Must((command, amount) => HaveValidCurrencyPrecision(command, amount))
            .WithMessage("TotalAmount has more decimal places than the currency allows.");
        RuleFor(x => x.CreatedAtUtc).NotEqual(default(DateTimeOffset));
    }

    private static bool BeSupportedCurrency(string? value)
    {
        try
        {
            _ = CurrencyCode.Create(value!);
            return true;
        }
        catch (DomainException)
        {
            return false;
        }
    }

    private static bool HaveValidCurrencyPrecision(
        CreateInvoicePaymentBalanceCommand command,
        decimal amount)
    {
        try
        {
            var currency = CurrencyCode.Create(command.CurrencyCode);
            return amount == decimal.Round(
                amount,
                currency.DecimalPlaces,
                MidpointRounding.ToEven);
        }
        catch (DomainException)
        {
            return true; // CurrencyCode has its own validation error.
        }
    }
}
