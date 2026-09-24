using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public class DeactivateCustomerCommandValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}
