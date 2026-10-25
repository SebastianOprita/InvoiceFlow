using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class DeactivateRoleCommandValidator : AbstractValidator<DeactivateRoleCommand>
{
    public DeactivateRoleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
