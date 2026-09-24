using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public sealed class CustomerContactValidator
    : AbstractValidator<CustomerContact>
{
    public CustomerContactValidator()
    {
        RuleFor(x => x.Email)
            .MaximumLength(64)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(15)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Email) ||
                !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Either email or phone is required.");
    }
}

public sealed class CustomerAddressValidator
    : AbstractValidator<CustomerAddress>
{
    public CustomerAddressValidator()
    {
        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.State)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(56);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20);
    }
}

public sealed class CustomerCreditPolicyValidator
    : AbstractValidator<CustomerCreditPolicy>
{
    public CustomerCreditPolicyValidator()
    {
        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaymentTermDays)
            .GreaterThanOrEqualTo(0);
    }
}

public sealed class CustomerTaxDetailsValidator
    : AbstractValidator<CustomerTaxDetails>
{
    public CustomerTaxDetailsValidator()
    {
        RuleFor(x => x.TaxNumber)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty()
            .MaximumLength(15);
    }
}
