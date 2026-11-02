using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public sealed class GenerateServiceClientTokenCommandValidator
    : AbstractValidator<GenerateServiceClientTokenCommand>
{
    public GenerateServiceClientTokenCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MaximumLength(ServiceClientId.MaxLength);

        RuleFor(x => x.ClientSecret)
            .NotEmpty();
    }
}
