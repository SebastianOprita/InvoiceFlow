using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

internal sealed class LoginPlatformUserCommandValidator : AbstractValidator<LoginPlatformUserCommand>
{
    public LoginPlatformUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();

        RuleFor(x => x.DeviceInfo)
            .MaximumLength(DeviceInfo.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceInfo));

        RuleFor(x => x.IpAddress)
            .MaximumLength(IpAddress.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));
    }
}