using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientSecretCommandValidator
    : AbstractValidator<RevokeServiceClientSecretCommand>
{
    public RevokeServiceClientSecretCommandValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();

        RuleFor(x => x.SecretId)
            .NotEmpty();
    }
}
