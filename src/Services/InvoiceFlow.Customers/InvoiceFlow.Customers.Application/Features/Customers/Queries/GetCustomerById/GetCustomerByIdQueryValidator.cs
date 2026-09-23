using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.CustomerId).NotEmpty();
    }
}
