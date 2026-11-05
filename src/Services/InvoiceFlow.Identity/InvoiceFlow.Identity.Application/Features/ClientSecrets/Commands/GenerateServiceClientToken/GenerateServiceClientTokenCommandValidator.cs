using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class GenerateServiceClientTokenCommandValidator
    : AbstractValidator<GenerateServiceClientTokenCommand>
{
    public GenerateServiceClientTokenCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MaximumLength(ServiceClientIdentifier.MaxLength);

        RuleFor(x => x.ClientSecret)
            .NotEmpty();
    }
}
