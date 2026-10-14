using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
