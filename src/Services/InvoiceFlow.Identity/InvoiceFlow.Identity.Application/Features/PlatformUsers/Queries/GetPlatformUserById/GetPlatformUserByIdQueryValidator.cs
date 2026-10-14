using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetPlatformUserByIdQueryValidator : AbstractValidator<GetPlatformUserByIdQuery>
{
    public GetPlatformUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
