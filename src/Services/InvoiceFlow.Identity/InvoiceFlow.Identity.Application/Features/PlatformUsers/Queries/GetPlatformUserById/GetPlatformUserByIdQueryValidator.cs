using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUserByIdQueryValidator : AbstractValidator<GetPlatformUserByIdQuery>
{
    public GetPlatformUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
