using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
