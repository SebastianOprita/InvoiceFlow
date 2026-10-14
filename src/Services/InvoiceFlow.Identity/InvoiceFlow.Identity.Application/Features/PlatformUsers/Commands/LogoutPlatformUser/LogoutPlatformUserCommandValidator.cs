using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class LogoutPlatformUserCommandValidator : AbstractValidator<LogoutPlatformUserCommand>
{
    public LogoutPlatformUserCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
