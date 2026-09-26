using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
