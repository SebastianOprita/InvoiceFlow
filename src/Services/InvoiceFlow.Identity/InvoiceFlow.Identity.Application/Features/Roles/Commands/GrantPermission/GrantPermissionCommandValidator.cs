using FluentValidation;
using InvoiceFlow.BuildingBlocks.Authorization;

namespace InvoiceFlow.Identity.Application;

public class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    public GrantPermissionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permission)
            .Must(p => p.IsValid())
            .WithMessage("Permission contain invalid flags.");
    }
}
