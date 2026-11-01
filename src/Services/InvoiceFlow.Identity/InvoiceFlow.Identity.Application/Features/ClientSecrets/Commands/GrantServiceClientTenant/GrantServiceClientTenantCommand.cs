using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GrantServiceClientTenantCommand(
    Guid ServiceClientId,
    Guid TenantId) : IRequest<Result>;
