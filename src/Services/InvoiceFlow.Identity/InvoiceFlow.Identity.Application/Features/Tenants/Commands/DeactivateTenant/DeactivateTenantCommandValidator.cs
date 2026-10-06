using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class DeactivateTenantCommandValidator : AbstractValidator<DeactivateTenantCommand>
{
    public DeactivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
