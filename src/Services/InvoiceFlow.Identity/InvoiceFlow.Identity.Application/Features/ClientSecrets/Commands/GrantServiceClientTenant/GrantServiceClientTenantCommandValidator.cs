using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GrantServiceClientTenantCommandValidator
    : AbstractValidator<GrantServiceClientTenantCommand>
{
    public GrantServiceClientTenantCommandValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();

        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
