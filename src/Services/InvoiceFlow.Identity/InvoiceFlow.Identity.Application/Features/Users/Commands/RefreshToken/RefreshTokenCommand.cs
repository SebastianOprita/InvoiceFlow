using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record RefreshTokenCommand(
    Guid TenantId,
    string RefreshToken,
    string? DeviceInfo,
    string? IpAddress)
    : IRequest<Result<RefreshTokenResponse>>;
