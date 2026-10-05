using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record RefreshPlatformTokenCommand(
    string RefreshToken,
    string? DeviceInfo,
    string? IpAddress)
    : IRequest<Result<RefreshPlatformTokenResponse>>;
