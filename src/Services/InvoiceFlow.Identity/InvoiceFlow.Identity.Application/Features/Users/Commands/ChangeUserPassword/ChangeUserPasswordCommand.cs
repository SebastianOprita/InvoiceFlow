using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record ChangeUserPasswordCommand(
    Guid TenantId,
    Guid UserId,
    string CurrentPassword,
    string NewPassword)
    : IRequest<Result>;
