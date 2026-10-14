using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
