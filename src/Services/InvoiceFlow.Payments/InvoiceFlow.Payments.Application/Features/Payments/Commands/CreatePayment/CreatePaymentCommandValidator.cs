using FluentValidation;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application.Features.Payments.Commands.CreatePayment;

internal sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CustomerCode).NotEmpty().MaximumLength(36);
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Must(BeSupportedCurrency)
            .WithMessage("CurrencyCode must be a supported ISO 4217 currency code.");
        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .Must((command, amount) => HaveValidCurrencyPrecision(command, amount))
            .WithMessage("Amount has more decimal places than the currency allows.");
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Reference).MaximumLength(PaymentReference.MaxLength);
        RuleFor(x => x.Notes).MaximumLength(PaymentNotes.MaxLength);
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

    private static bool HaveValidCurrencyPrecision(CreatePaymentCommand command, decimal amount)
    {
        try
        {
            var currency = CurrencyCode.Create(command.CurrencyCode);
            return amount == decimal.Round(amount, currency.DecimalPlaces, MidpointRounding.ToEven);
        }
        catch (DomainException)
        {
            return true; // CurrencyCode has its own validation error.
        }
    }
}
