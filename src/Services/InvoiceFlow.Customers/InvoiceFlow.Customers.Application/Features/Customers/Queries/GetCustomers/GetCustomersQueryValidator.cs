using FluentValidation;

namespace InvoiceFlow.Customers.Application;

public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
