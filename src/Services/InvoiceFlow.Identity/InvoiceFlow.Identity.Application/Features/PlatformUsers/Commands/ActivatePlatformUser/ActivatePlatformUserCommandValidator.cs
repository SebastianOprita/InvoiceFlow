using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class ActivatePlatformUserCommandValidator : AbstractValidator<ActivatePlatformUserCommand>
{
    public ActivatePlatformUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
