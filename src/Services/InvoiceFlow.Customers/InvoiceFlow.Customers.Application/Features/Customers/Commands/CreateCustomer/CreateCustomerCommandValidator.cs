using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.CustomerCode)
            .NotEmpty()
            .MaximumLength(38);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.CustomerContact)
            .NotNull()
            .SetValidator(new CustomerContactValidator());

        RuleFor(x => x.CustomerTaxDetails)
            .NotNull()
            .SetValidator(new CustomerTaxDetailsValidator());

        RuleFor(x => x.CustomerAddress)
            .NotNull()
            .SetValidator(new CustomerAddressValidator());

        RuleFor(x => x.CustomerCreditPolicy)
            .NotNull()
            .SetValidator(new CustomerCreditPolicyValidator());
    }
}
