using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
    }
}
