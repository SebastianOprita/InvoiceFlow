using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientTenantCommandValidator
    : AbstractValidator<RevokeServiceClientTenantCommand>
{
    public RevokeServiceClientTenantCommandValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();

        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
