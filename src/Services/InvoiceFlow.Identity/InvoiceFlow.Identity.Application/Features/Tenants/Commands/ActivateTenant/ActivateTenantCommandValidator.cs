using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
