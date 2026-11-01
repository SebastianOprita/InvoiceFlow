using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class CreateServiceClientSecretCommandValidator
    : AbstractValidator<CreateServiceClientSecretCommand>
{
    public CreateServiceClientSecretCommandValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ServiceClientSecretName.MaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ServiceClientSecretDescription.MaxLength)
            .When(x => x.Description is not null);

        RuleFor(x => x.ExpiresAtUtc)
            .NotEmpty();
    }
}
