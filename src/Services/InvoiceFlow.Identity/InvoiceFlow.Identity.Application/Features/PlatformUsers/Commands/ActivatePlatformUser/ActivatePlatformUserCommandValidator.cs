using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class ActivatePlatformUserCommandValidator : AbstractValidator<ActivatePlatformUserCommand>
{
    public ActivatePlatformUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
