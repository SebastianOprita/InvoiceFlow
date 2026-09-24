using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(64)
            .When(x => x.Name != null);

        RuleFor(x => x.CustomerContact!)
            .SetValidator(new CustomerContactValidator())
            .When(x => x.CustomerContact != null);

        RuleFor(x => x.CustomerAddress!)
            .SetValidator(new CustomerAddressValidator())
            .When(x => x.CustomerAddress != null);

        RuleFor(x => x.CustomerCreditPolicy!)
            .SetValidator(new CustomerCreditPolicyValidator())
            .When(x => x.CustomerCreditPolicy != null);
    }
}
