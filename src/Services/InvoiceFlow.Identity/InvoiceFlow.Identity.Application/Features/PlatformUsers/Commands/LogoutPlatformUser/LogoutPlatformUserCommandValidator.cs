using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class LogoutPlatformUserCommandValidator : AbstractValidator<LogoutPlatformUserCommand>
{
    public LogoutPlatformUserCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
