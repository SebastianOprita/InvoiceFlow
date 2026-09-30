using FluentValidation;
using InvoiceFlow.BuildingBlocks.Authorization;

namespace InvoiceFlow.Identity.Application;

public class UpdatePermissionsCommandValidator : AbstractValidator<UpdatePermissionsCommand>
{
    public UpdatePermissionsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permissions)
            .Must(p => p.IsValid())
            .WithMessage("Permissions contain invalid flags.");
    }
}
