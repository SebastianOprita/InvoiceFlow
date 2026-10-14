using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record ImpersonateUserCommand(
    Guid ActorUserId,
    Guid TargetUserTenantId,
    string TargetUserEmail,
    string? Reason,
    string? DeviceInfo,
    string? IpAddress)
    : IRequest<Result<ImpersonateUserCommandResponse>>;
