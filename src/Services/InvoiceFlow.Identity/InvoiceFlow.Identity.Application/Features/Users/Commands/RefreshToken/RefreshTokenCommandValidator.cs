using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

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
