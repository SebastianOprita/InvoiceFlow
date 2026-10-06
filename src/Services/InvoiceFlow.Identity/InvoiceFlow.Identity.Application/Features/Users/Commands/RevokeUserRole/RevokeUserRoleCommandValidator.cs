using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class RevokeUserRoleCommandValidator : AbstractValidator<RevokeUserRoleCommand>
{
    public RevokeUserRoleCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}
