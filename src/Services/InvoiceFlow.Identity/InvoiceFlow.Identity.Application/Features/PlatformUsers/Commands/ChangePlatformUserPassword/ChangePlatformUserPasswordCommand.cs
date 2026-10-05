using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record ChangePlatformUserPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword)
    : IRequest<Result>;
