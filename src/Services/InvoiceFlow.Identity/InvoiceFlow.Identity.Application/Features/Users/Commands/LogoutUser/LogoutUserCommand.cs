using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record LogoutUserCommand(
    Guid TenantId,
    string RefreshToken)
    : IRequest<Result>;
