using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record LogoutUserCommand(
    Guid TenantId,
    string RefreshToken)
    : IRequest<Result>;
