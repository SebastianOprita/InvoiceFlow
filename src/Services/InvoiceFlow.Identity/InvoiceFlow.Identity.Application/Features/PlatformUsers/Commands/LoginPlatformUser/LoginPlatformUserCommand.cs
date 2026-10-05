using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record LoginPlatformUserCommand(
    string Email,
    string Password,
    string? DeviceInfo,
    string? IpAddress)
    : IRequest<Result<LoginPlatformUserCommandResponse>>;
