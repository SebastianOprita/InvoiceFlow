using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record ChangeUserPasswordCommand(
    Guid TenantId,
    Guid UserId,
    string CurrentPassword,
    string NewPassword)
    : IRequest<Result>;
