using FluentValidation;

namespace InvoiceFlow.Customers.Application;

internal sealed class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
    }
}
