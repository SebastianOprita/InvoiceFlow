using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientPermissionsCommandValidator
    : AbstractValidator<RevokeServiceClientPermissionsCommand>
{
    public RevokeServiceClientPermissionsCommandValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();

        RuleFor(x => x.Permission)
            .GreaterThan(0)
            .Must(p => Permissions.IsSinglePermission(p))
            .WithMessage("Exactly one valid permission is required.");
    }
}
