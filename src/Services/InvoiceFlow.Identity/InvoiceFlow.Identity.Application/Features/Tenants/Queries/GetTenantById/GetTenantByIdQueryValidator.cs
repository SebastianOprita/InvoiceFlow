using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
