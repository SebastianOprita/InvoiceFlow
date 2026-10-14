using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permission)
            .Must(p => RolePermissions.IsValid(p))
            .WithMessage("Permission contain invalid flags.");
    }
}
