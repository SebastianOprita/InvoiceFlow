using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class ActivateRoleCommandValidator : AbstractValidator<ActivateRoleCommand>
{
    public ActivateRoleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
