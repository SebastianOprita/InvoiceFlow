using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public class ActivateCustomerCommandValidator : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}
