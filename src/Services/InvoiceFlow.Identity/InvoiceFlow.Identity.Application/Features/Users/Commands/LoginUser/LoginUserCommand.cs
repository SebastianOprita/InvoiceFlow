using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record LoginUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string? DeviceInfo,
    string? IpAddress)
    : IRequest<Result<LoginUserCommandResponse>>;
