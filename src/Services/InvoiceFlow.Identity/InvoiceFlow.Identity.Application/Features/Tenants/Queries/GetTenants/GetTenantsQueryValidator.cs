using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
    }
}
