using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public class RefreshPlatformTokenCommandValidator : AbstractValidator<RefreshPlatformTokenCommand>
{
    public RefreshPlatformTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.DeviceInfo)
            .MaximumLength(DeviceInfo.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceInfo));

        RuleFor(x => x.IpAddress)
            .MaximumLength(IpAddress.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));
    }
}
