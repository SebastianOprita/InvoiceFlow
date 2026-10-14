using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
