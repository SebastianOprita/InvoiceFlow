using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
