using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class DeactivateTenantCommandValidator : AbstractValidator<DeactivateTenantCommand>
{
    public DeactivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
