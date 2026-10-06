using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
