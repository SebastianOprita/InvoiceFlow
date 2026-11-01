using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record RevokeServiceClientTenantCommand(
    Guid ServiceClientId,
    Guid TenantId) : IRequest<Result>;
