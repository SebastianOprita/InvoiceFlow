using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    public GrantPermissionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permission)
            .Must(p => RolePermissions.IsSinglePermission(p))
            .WithMessage("Exactly one valid permission is required.");
    }
}
