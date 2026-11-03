using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetServiceClientSecretsQueryValidator
    : AbstractValidator<GetServiceClientSecretsQuery>
{
    public GetServiceClientSecretsQueryValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();
    }
}
