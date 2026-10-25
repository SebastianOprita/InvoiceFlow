using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(RoleDescription.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Permissions)
            .Must(p => RolePermissions.IsSinglePermission(p))
            .WithMessage("Exactly one valid permission is required.");
    }
}
