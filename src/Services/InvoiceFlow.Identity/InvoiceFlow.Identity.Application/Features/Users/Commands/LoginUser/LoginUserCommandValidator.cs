using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

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
