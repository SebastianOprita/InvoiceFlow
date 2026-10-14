using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
